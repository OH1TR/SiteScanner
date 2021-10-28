using DataModel.ModuleInterface;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ScannerBot.Modules.Screenshot
{
    class WebDriver
    {
        IWebDriver driver;

        public WebDriver(IConfig config,string proxy,int width=1920,int height=2000)
        {
            FirefoxOptions options = new FirefoxOptions();
            //options.AddArguments("--headless");
            options.AddArguments("--width="+ width.ToString());
            options.AddArguments("--height="+height.ToString());

            var profileManager = new FirefoxProfileManager();
            var e=profileManager.ExistingProfiles;
            FirefoxProfile profile = profileManager.GetProfile(config.GetSetting(typeof(Screenshot), "FirefoxProfile"));
            
            //profile.SetProxyPreferences(seleniumProxy);
            //profile.AcceptUntrustedCertificates = true;

            options.Profile = profile;
            driver = new FirefoxDriver(options);
        }
        

        public void OpenURL(string url)
        {
                driver.Navigate().GoToUrl(url);
        }

        public void Close()
        {
            driver.Close();
        }

        public void Screenshot(string path)
        {
            var ss = ((ITakesScreenshot)driver).GetScreenshot();
            ss.SaveAsFile(path, ScreenshotImageFormat.Png);
        }
    }
}
