using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Core.Models
{
    /// <summary>
    /// 统一爬取结果模型
    /// </summary>
    /// <typeparam name="T">条目类型</typeparam>
    public class ScraperResult<T> where T : class
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ScraperResult<T> Success(List<T> data) => new ScraperResult<T>
        {
            IsSuccess = true,
            Data = data,
            UpdatedTime = DateTime.Now
        };

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ScraperResult<T> Failure(string errorMessage) => new ScraperResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            UpdatedTime = DateTime.Now
        };
    }
}
