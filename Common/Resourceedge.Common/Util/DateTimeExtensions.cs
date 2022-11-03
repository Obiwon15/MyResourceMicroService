using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Util
{
    public static class DateTimeExtensions
    {
        public static bool InRange(this DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck.Date >= startDate.Date && dateToCheck.Date <= endDate.Date;
        }

        public static bool InRange(this DateTime dateToCheck, DateTime? startDate, DateTime? endDate)
        {

            ValidateDateRange(startDate, endDate);

            bool matchedStartCondition = true;
            bool matchedEndCondition = true;
            if (startDate.HasValue)
            {
                matchedStartCondition = dateToCheck.Date >= startDate.Value.Date;
            }

            if (endDate.HasValue)
            {
                matchedEndCondition = dateToCheck.Date <= endDate.Value.Date;
            }


            return matchedStartCondition && matchedEndCondition;
        }


        public static bool WithinTime(this DateTime dateToCheck, DateTime? startDate, DateTime? endDate)
        {

            ValidateDateRange(startDate, endDate);

            bool matchedStartCondition = true;
            bool matchedEndCondition = true;
            if (startDate.HasValue)
            {
                matchedStartCondition = dateToCheck >= startDate.Value;
            }

            if (endDate.HasValue)
            {
                matchedEndCondition = dateToCheck <= endDate.Value;
            }


            return matchedStartCondition && matchedEndCondition;
        }


        public static bool DateEquals(this DateTime dateToCheck, DateTime? dateUnderConsideration)
        {
            if (dateUnderConsideration == null)
                return false;

            if (dateToCheck.Date != dateUnderConsideration.Value.Date)
                return false;

            return true;

        }
        public static double ToTimeStamp(this DateTime dateInstance)
        {
            DateTime epochDateTime = new DateTime(1970, 1, 1);
            return (dateInstance - epochDateTime).TotalMilliseconds;
        }


        public static void ValidateDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate > endDate)
                    throw new InvalidOperationException("Invalid Date Arguments, Start Date Cannot Be Greater Than End Date");
            }

        }
    }
}
