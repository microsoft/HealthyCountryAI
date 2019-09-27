namespace SixSeasons.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public static class AsyncFileCopy
    {
        public static async Task CopyFiles(Dictionary<string, string> files, Action<double> progressCallback)
        {
            long total_size = files.Keys.Select(x => new FileInfo(x).Length).Sum();

            long total_read = 0;

            double progress_size = 10000.0;

            foreach (var item in files)
            {
                long total_read_for_file = 0;

                var from = item.Key;
                var to = item.Value;

                using (var outStream = new FileStream(to, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (var inStream = new FileStream(from, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await CopyStream(inStream, outStream, x =>
                        {
                            total_read_for_file = x;
                            progressCallback(((total_read + total_read_for_file) / (double)total_size) * progress_size);
                        });
                    }
                }

                total_read += total_read_for_file;
            }
        }

        public static async Task CopyStream(Stream from, Stream to, Action<long> progress)
        {
            int buffer_size = 10240;

            byte[] buffer = new byte[buffer_size];

            long total_read = 0;

            while (total_read < from.Length)
            {
                int read = await from.ReadAsync(buffer, 0, buffer_size);

                await to.WriteAsync(buffer, 0, read);

                total_read += read;

                progress(total_read);
            }
        }
    }
}
