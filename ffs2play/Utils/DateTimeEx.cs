/****************************************************************************
**
** Copyright (C) 2017 FSFranceSimulateur team.
** Contact: https://github.com/ffs2/ffs2play
**
** FFS2Play is free software; you can redistribute it and/or modify
** it under the terms of the GNU General Public License as published by
** the Free Software Foundation; either version 3 of the License, or
** (at your option) any later version.
**
** FFS2Play is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** The license is as published by the Free Software
** Foundation and appearing in the file LICENSE.GPL3
** included in the packaging of this software. Please review the following
** information to ensure the GNU General Public License requirements will
** be met: https://www.gnu.org/licenses/gpl-3.0.html.
****************************************************************************/

/****************************************************************************
 * DateTimeEx.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/

using System;
using System.Diagnostics;

namespace ffs2play
{
    public class DateTimeEx
    {
        private static DateTime _startTime;
        private static Stopwatch _stopWatch = null;
        private static TimeSpan _maxIdle = TimeSpan.FromSeconds(60);
        private static readonly DateTime UnixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UtcNow
        {
            get
            {
                if ((_stopWatch == null) ||
                    (_startTime.Add(_maxIdle) < DateTime.UtcNow))
                {
                    Reset();
                }
                return _startTime.AddTicks(_stopWatch.Elapsed.Ticks);
            }
        }

        public static long UtcNowMilli
        {
            get
            {
                return (UtcNow.Ticks - UnixEpoch.Ticks) / TimeSpan.TicksPerMillisecond;
            }
        }

        private static void Reset()
        {
            _startTime = DateTime.UtcNow;
            _stopWatch = Stopwatch.StartNew();
        }

        public static long UnixTimestampFromDateTime(DateTime date)
        {
            return (date.Ticks - UnixEpoch.Ticks) / TimeSpan.TicksPerMillisecond;
        }

        public static DateTime TimeFromUnixTimestamp(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }
    }
}
