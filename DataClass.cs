using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEXInstaller
{
    public class DataClass
    {
        public string TermsOfService = "Why is it important to have the terms and conditions on my website?If something goes wrong and someone sues you, having terms and conditions in place can help limit your liability. They can also help deter people from misusing your website. If people know that there are consequences for misuse, they may be less likely to do so.";
        public string NameInTerms = "Mr. Omar Khaled Raeia - Physics Pros";
        public  Image Logo = PEXInstaller.Properties.Resources.full_logo;
        public string SelectedDirectory { get; set; }

    }
}
