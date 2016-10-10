using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Security.Keystore;
using Android.Support.V4.App;
using Java.Net;
using Java.Security;
using Javax.Crypto.Spec;
using Newtonsoft.Json.Linq;
using Uri = Android.Net.Uri;

namespace IssueApp.Droid
{
    [Activity(Label = "IssueApp.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : FragmentActivity, IOnMapReadyCallback
    {
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

            LoadIssues(googleMap);
        }

        private async void LoadIssues(GoogleMap googleMap)
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
                        LoadIssues(googleMap);
                    }
                    else
                        LoadIssues(googleMap);
                }).SetNegativeButton(Properties.Resources.Host + "を開く", (sender, args) =>
                {
                    var uri = Uri.Parse(Properties.Resources.Url);
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                    LoadIssues(googleMap);
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
            foreach (var issue in response.issues)
            {
                if (double.IsNaN(issue.lat)) continue;

                var markerOptions = new MarkerOptions();
                markerOptions.SetPosition(new LatLng(issue.lat, issue.lng));
                markerOptions.SetTitle(issue.subject);
                googleMap.AddMarker(markerOptions);
            }

            var listView = FindViewById<ListView>(Resource.Id.listView);
            var adapter = new IssuesAdapter(Application.Context);
            adapter.AddAll(response.issues);
            listView.Adapter = adapter;

            progressDialog.Hide();
        }
    }
}


