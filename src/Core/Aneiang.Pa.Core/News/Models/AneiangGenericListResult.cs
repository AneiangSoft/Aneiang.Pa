using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Core.News.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AneiangGenericListResult<T> where T : class
    {
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// 初始化结果
        /// </summary>
        /// <param name="isSuccessd">是否成功</param>
        /// <param name="errorMessage">错误消息</param>
        public AneiangGenericListResult(bool isSuccessd = true, string? errorMessage = null)
        {
            IsSuccessd = isSuccessd;
            ErrorMessage = errorMessage;
            UpdatedTime = DateTime.Now;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessd { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <returns>成功的结果对象</returns>
        public static AneiangGenericListResult<T> Success() => new AneiangGenericListResult<T>(true);

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <returns>失败的结果对象</returns>
        public static AneiangGenericListResult<T> Failure(string errorMessage) => new AneiangGenericListResult<T>(false, errorMessage);
    }

    public class AneiangGenericResult<T> where T : class
    {
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// 初始化结果
        /// </summary>
        /// <param name="isSuccessd">是否成功</param>
        /// <param name="errorMessage">错误消息</param>
        public AneiangGenericResult(bool isSuccessd = true, string? errorMessage = null)
        {
            IsSuccessd = isSuccessd;
            ErrorMessage = errorMessage;
            UpdatedTime = DateTime.Now;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessd { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public T? Data { get; set; } = default(T);

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <returns>成功的结果对象</returns>
        public static AneiangGenericResult<T> Success() => new AneiangGenericResult<T>(true);

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <returns>失败的结果对象</returns>
        public static AneiangGenericResult<T> Failure(string errorMessage) => new AneiangGenericResult<T>(false, errorMessage);
    }
}
