using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WorkFinder.Web.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Chuyển đổi chuỗi thành slug URL thân thiện với SEO
        /// </summary>
        /// <param name="text">Chuỗi cần chuyển đổi</param>
        /// <returns>Slug URL thân thiện với SEO</returns>
        public static string ToSlug(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Chuyển thành chữ thường
            text = text.ToLowerInvariant();

            // Chuyển đổi dấu thành không dấu
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            
            // Chuẩn hóa chuỗi
            text = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Thay thế các ký tự không phải chữ cái và số bằng dấu gạch ngang
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            
            // Loại bỏ nhiều dấu cách liên tiếp
            text = Regex.Replace(text, @"\s+", " ").Trim();
            
            // Thay thế khoảng trắng bằng dấu gạch ngang
            text = Regex.Replace(text, @"\s", "-");
            
            // Loại bỏ nhiều dấu gạch ngang liên tiếp
            text = Regex.Replace(text, @"-+", "-");
            
            // Cắt bớt nếu dài hơn 100 ký tự
            if (text.Length > 100)
                text = text.Substring(0, 100);
            
            return text;
        }
    }
} 