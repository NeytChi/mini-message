using System;
using NUnit.Framework;
using Common.NDatabase;

namespace Common
{
    [TestFixture()]
    public class FilesControlTest
    {
        [Test()]
        public void SetUpWordsContent()
        { 
            Database.Initialization(false);
            FileControl.SetUpWordsContent();
        }
    }
}