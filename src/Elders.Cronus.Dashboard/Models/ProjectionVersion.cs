﻿namespace Elders.Cronus.Dashboard.Models
{
    public class ProjectionVersion
    {
        public ProjectionVersion(string hash, int revision, string status)
        {
            Hash = hash;
            Revision = revision;
            Status = status;
        }

        public string Hash { get; set; }


        public int Revision { get; set; }

        public string Status { get; set; }

        public override string ToString()
        {
            return $"{Status}-{Hash}-{Revision}";
        }
    }
}
