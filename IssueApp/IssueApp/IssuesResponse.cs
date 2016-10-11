using System;

namespace IssueApp
{
    public class IssuesResponse
    {
        public Issue[] issues { get; set; }
        public int total_count { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
    }

    public class Issue
    {
        public int id { get; set; }
        public Project project { get; set; }
        public Tracker tracker { get; set; }
        public Status status { get; set; }
        public Priority priority { get; set; }
        public Author author { get; set; }
        public Assigned_To assigned_to { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public int done_ratio { get; set; }
        public DateTime created_on { get; set; }
        public DateTime updated_on { get; set; }
        public Category category { get; set; }
        public string start_date { get; set; }
        public string due_date { get; set; }
        public Custom_Fields[] custom_fields { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public int distance { get; set; }
    }

    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Tracker
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Status
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Priority
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Author
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Assigned_To
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Custom_Fields
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool multiple { get; set; }
        public object value { get; set; }
    }
}
