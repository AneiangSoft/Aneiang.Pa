using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Aneiang.Pa.Core.Data
{
    public interface IExtendableObject
    {
        public string ExtensionData { get; set; }
    }
}
