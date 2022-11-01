using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCT.Common.Services
{
    public interface IStatusBarService
    {
        void BeginProgress(int max, string text);
        void UpdateProgress(int v);
        void EndProgress();
    }
}
