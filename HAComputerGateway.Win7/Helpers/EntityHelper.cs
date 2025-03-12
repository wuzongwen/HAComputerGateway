using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HAComputerGateway.Win7.Helpers
{
    public class EntityHelper
    {
        /// <summary>
        /// 判断实体中所有公共属性和字段是否都有值
        /// </summary>
        /// <param name="entity">要验证的实体对象</param>
        /// <returns>如果所有属性和字段都有值则返回 true，否则返回 false</returns>
        public static bool IsEntityComplete(object entity)
        {
            if (entity == null) return false;

            Type type = entity.GetType();

            // 检查公共属性
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = prop.GetValue(entity, null);
                if (value == null)
                {
                    return false;
                }
                if (value is string str && string.IsNullOrWhiteSpace(str))
                {
                    return false;
                }
            }

            // 检查公共字段
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(entity);
                if (value == null)
                {
                    return false;
                }
                if (value is string str && string.IsNullOrWhiteSpace(str))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
