using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace IssueApp.Droid
{
    class IssuesAdapter : ArrayAdapter<Issue>
    {
        private readonly LayoutInflater _layoutInflater;
        private PackageManager _contextPackageManager;

        public IssuesAdapter(Context context) : base(context, 0)
        {
            _layoutInflater = LayoutInflater.From(context);
            _contextPackageManager = context.PackageManager;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
                convertView = _layoutInflater.Inflate(Resource.Layout.ListViewItemTemplate, parent, false);

            var issue = GetItem(position);

            var subject = convertView.FindViewById<TextView>(Resource.Id.subject);
            subject.SetText(issue.subject, TextView.BufferType.Normal);

            var description = convertView.FindViewById<TextView>(Resource.Id.description);
            description.SetText(issue.description, TextView.BufferType.Normal);

            var date = convertView.FindViewById<TextView>(Resource.Id.date);
            date.SetText(issue.start_date, TextView.BufferType.Normal);

            return convertView;
        }
    }
}