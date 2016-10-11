using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Security.Keystore;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Java.Net;
using Java.Security;
using Javax.Crypto.Spec;
using Newtonsoft.Json.Linq;
using Permission = Android.Content.PM.Permission;
using Uri = Android.Net.Uri;

namespace IssueApp.Droid
{
    [Activity(Label = "IssueApp.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : FragmentActivity, IOnMapReadyCallback, GoogleApiClient.IConnectionCallbacks
    {
        private GoogleMap _googleMap;
        private GoogleApiClient _googleApiClient;
        private Issue[] _issues;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var mapFragment = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map);
            mapFragment.GetMapAsync(this);


        }

        public void OnMapReady(GoogleMap googleMap)
        {
            googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(35.68, 139.76), 10.0f));

            _googleMap = googleMap;

            LoadIssues();
        }

        private async void LoadIssues()
        {
            var sharedPreferences = GetSharedPreferences("IssueApp", FileCreationMode.Private);
            var key = sharedPreferences.GetString("Key", null);
            if (key == null)
            {
                var editText = new EditText(this);
                var alertDialog = new AlertDialog.Builder(this);
                alertDialog.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                alertDialog.SetTitle("Redmine認証キー").SetView(editText).SetPositiveButton("OK", async (sender, args) =>
                {
                    var service = new RedmineService(editText.Text);
                    var check = await service.CheckAsync();
                    if (check)
                    {
                        var preferencesEditor = sharedPreferences.Edit();
                        preferencesEditor.PutString("Key", editText.Text);
                        preferencesEditor.Apply();
                        LoadIssues();
                    }
                    else
                        LoadIssues();
                }).SetNegativeButton(Properties.Resources.Host + "を開く", (sender, args) =>
                {
                    var uri = Uri.Parse(Properties.Resources.Url);
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                    LoadIssues();
                }).Show();
                return;
            }

            var progressDialog = new ProgressDialog(this);
            progressDialog.SetMessage("Redmineから読み込み中");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialog.SetButton("この表示を消す", (sender, args) => progressDialog.Hide());
            progressDialog.Show();

            var redmineService = new RedmineService(key);
            var response = await redmineService.GetIssues();
            _issues = response.issues;

            foreach (var issue in _issues)
            {
                if (double.IsNaN(issue.lat)) continue;

                var markerOptions = new MarkerOptions();
                markerOptions.SetPosition(new LatLng(issue.lat, issue.lng));
                markerOptions.SetTitle(issue.subject);
                _googleMap.AddMarker(markerOptions);
            }

            UpdateList();

            progressDialog.Hide();

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                _googleMap.MyLocationEnabled = true;
            }
            else
            {
                RequestPermissions(new[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation }, 0x01);
            }

            _googleApiClient = new GoogleApiClient.Builder(this)
                .AddApi(LocationServices.API)
                .AddConnectionCallbacks(this)
                .Build();
            await Task.Run(() =>
            {
                _googleApiClient.BlockingConnect();
            });
        }

        private void UpdateList()
        {
            var listView = FindViewById<ListView>(Resource.Id.listView);
            var adapter = new IssuesAdapter(Application.Context);
            adapter.AddAll(_issues.OrderBy(x => x.distance).ToList());
            listView.Adapter = adapter;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (grantResults[0] == Permission.Granted && grantResults[1] == Permission.Granted)
            {
                _googleMap.MyLocationEnabled = true;
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            Console.WriteLine("Connected!!!");
            var lastLocation = LocationServices.FusedLocationApi.GetLastLocation(_googleApiClient);
            foreach (var issue in _issues)
            {
                if (double.IsNaN(issue.lat)) continue;

                float[] results = new float[3];
                Location.DistanceBetween(lastLocation.Latitude, lastLocation.Longitude, issue.lat, issue.lng, results);
                issue.distance = (int)results[0];

                Console.WriteLine("distance:" + issue.distance);
            }
            UpdateList();
        }

        public void OnConnectionSuspended(int cause)
        {
            throw new NotImplementedException();
        }
    }
}


