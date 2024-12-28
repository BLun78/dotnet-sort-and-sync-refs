using DotnetSortAndSyncRefs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Frameworks;

namespace DotnetSortAndSyncRefs.Models
{
    internal class ItemGroup
    {
        public ItemGroup(XElement element)
        {
            Element = CheckAndChooseItemGroup(element);
            FindCondition();
        }

        public XElement Element { get; private set; }
        public bool HasCondition { get; private set; }
        public string Condition { get; private set; }
        public string Framework { get; private set; }
        public NuGetFramework NuGetFramework { get; private set; }

        private void FindCondition()
        {
            var condition = Element.FirstAttribute;
            if (condition != null &&
                condition.Name == ConstConfig.Condition)
            {
                Condition = condition.Value;
                HasCondition = true;
                SetFramework();
            }
        }

        private void SetFramework()
        {
            var splitArray = Condition.Split(@"'");
            if (splitArray.Length == 5)
            {
                var framework = splitArray[3];
                NuGetFramework = new NuGetFramework(framework);
                Framework = framework;
            }
        }

        private XElement CheckAndChooseItemGroup(XElement element)
        {
            if (element.Name == ConstConfig.ItemGroup)
            {
                return element;
            }
            else if (element.Parent != null && element.Parent.Name == ConstConfig.ItemGroup)
            {
                return element.Parent;
            }
            throw new ArgumentOutOfRangeException($"The Element or his Parent Element is no {ConstConfig.ItemGroup}!");
        }
    }
}
