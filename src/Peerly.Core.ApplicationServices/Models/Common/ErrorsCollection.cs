using System.Collections.Generic;

namespace Peerly.Core.ApplicationServices.Models.Common;

public sealed class ErrorsCollection : Dictionary<string, string[]>
{
    public bool HasErrors => Count > 0;
}
