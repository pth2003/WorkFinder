using System;
using System.Collections.Generic;
using System.Text.Json;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Extensions
{
    public static class JobExtensions
    {
        /// <summary>
        /// Lấy giá trị từ metadata của job
        /// </summary>
        /// <param name="job">Đối tượng job</param>
        /// <param name="key">Khóa cần lấy</param>
        /// <param name="defaultValue">Giá trị mặc định nếu không tìm thấy</param>
        /// <returns>Giá trị được lưu trong metadata</returns>
        public static string GetMetadataValue(this Job job, string key, string defaultValue = "")
        {
            if (job == null || string.IsNullOrEmpty(job.Metadata))
                return defaultValue;

            try
            {
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(job.Metadata);
                if (metadata != null && metadata.ContainsKey(key))
                {
                    return metadata[key];
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi và trả về giá trị mặc định
            }

            return defaultValue;
        }

        /// <summary>
        /// Lấy toàn bộ metadata của job dưới dạng Dictionary
        /// </summary>
        /// <param name="job">Đối tượng job</param>
        /// <returns>Dictionary chứa metadata</returns>
        public static Dictionary<string, string> GetAllMetadata(this Job job)
        {
            if (job == null || string.IsNullOrEmpty(job.Metadata))
                return new Dictionary<string, string>();

            try
            {
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(job.Metadata);
                return metadata ?? new Dictionary<string, string>();
            }
            catch (Exception)
            {
                // Bỏ qua lỗi và trả về dictionary rỗng
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Kiểm tra xem job có metadata cho khóa cụ thể không
        /// </summary>
        /// <param name="job">Đối tượng job</param>
        /// <param name="key">Khóa cần kiểm tra</param>
        /// <returns>true nếu tồn tại, false nếu không</returns>
        public static bool HasMetadata(this Job job, string key)
        {
            if (job == null || string.IsNullOrEmpty(job.Metadata))
                return false;

            try
            {
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(job.Metadata);
                return metadata != null && metadata.ContainsKey(key);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}