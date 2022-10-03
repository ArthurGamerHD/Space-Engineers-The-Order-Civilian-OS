﻿using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public class TimeAgo
        {
            public String Time;
            public TimeAgo(DateTime Reference)
            {
                string R;
                var T = DateTime.Now.Subtract(Reference);
                if (T <= TimeSpan.FromSeconds(60))
                {
                    R = string.Format("{0} seconds", T.Seconds);
                }
                else if (T <= TimeSpan.FromMinutes(60))
                {
                    R = T.Minutes > 1 ?
                        String.Format("{0} minutes", T.Minutes) :
                       "a minute";
                }
                else if (T <= TimeSpan.FromHours(24))
                {
                    R = T.Hours > 1 ?
                        String.Format("{0} hours", T.Hours) :
                       "an hour";
                }
                else if (T <= TimeSpan.FromDays(30))
                {
                    R = T.Days > 1 ?
                        String.Format("{0} days", T.Days) :
                       "a day";
                }
                else if (T <= TimeSpan.FromDays(365))
                {
                    R = T.Days > 30 ?
                        String.Format("{0} months", T.Days / 30) :
                       "a month";
                }
                else
                {
                    R = T.Days > 365 ?
                        String.Format("{0} years", T.Days / 365) :
                       "a year";
                }
                Time = $"About {R} ago";
            }
        }
    }
}
