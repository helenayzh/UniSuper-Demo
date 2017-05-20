using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using OpenQA.Selenium.Interactions;
using System.Configuration;

namespace UniSuper
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }
    }
    [TestClass]
    public class UniDemoClass
    {
        private string baseURL;
        private RemoteWebDriver driver;
        private string browser;
        public TestContext TestContext { get; set; }
        public enum ItemSatus
        {
            All,
            Active,
            Completed
        }


        public static void WaitForLoad(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            int timeoutSec = 15;
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeoutSec));
            wait.Until(wd => js.ExecuteScript("return document.readyState").ToString() == "complete");
        }

        //Open http://todomvc.com and click AngularJSR link
        public static void openPage(RemoteWebDriver driver, string baseURL, string pageName)
        {
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
            driver.Navigate().GoToUrl(baseURL);
            WaitForLoad(driver);
            var links = driver.FindElementsByClassName("routing");
            //findelementbylinktext("AngularJSR") not working
            foreach (IWebElement link in links)
            {
                if (link.Text == pageName)
                {
                    link.FindElement(By.TagName("a")).Click();
                    WaitForLoad(driver);
                    return;
                }
            }
            
        }

        //Get the list of To-Do itmes
        public static IList<IWebElement> getToDoList(RemoteWebDriver driver)
        {
            IWebElement listContainer = driver.FindElement(By.Id("todo-list"));
            IList<IWebElement> listItems = listContainer.FindElements(By.TagName("li"));
            if (listItems.Count > 0)
            {
                return listItems;
            }
            else
            {
                return null;
            } 
        }

        //Add a To-Do item into the list
        public static void addToDoItem(RemoteWebDriver driver, string itemName)
        {
            IWebElement todoInputBox = driver.FindElementById("new-todo");
            todoInputBox.SendKeys(itemName);
            todoInputBox.SendKeys(Keys.Return);
            WaitForLoad(driver);
        }

        //Check if a To-Do item is in the list
        public static Boolean verifyToDoItemExists(RemoteWebDriver driver, string itemName)
        {
            IList < IWebElement > items = getToDoList(driver);
            if (items != null)
            {
                foreach (IWebElement item in items)
                {
                    if(item.Text == itemName)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        //Returm a specific To-Do item as IWebElement
        public static IWebElement getToDoItem(RemoteWebDriver driver, string itemName)
        {
            IList<IWebElement> items = getToDoList(driver);
            if (items != null)
            {
                foreach (IWebElement item in items)
                {
                    if (item.Text == itemName)
                    {
                        return item;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        //Click the checkbox next to the specific To-Do item
        public static IWebElement clickToDoItemCheckbox(RemoteWebDriver driver, string itemName)
        {
            IList<IWebElement> items = getToDoList(driver);
            if (items != null)
            {
                foreach (IWebElement item in items)
                {
                    if (item.Text == itemName)
                    {
                        IWebElement itemCheckbox = item.FindElement(By.XPath(".//input[@type='checkbox']"));
                        itemCheckbox.Click();
                        WaitForLoad(driver);
                        return itemCheckbox;
                    }
                }
                return null;

            }
            else
            {
                return null;
            }
        }

        //Get the checkbox next to the To-Do item
        public static IWebElement getToDoItemCheckBox(RemoteWebDriver driver, string itemName)
        {
            IList<IWebElement> items = getToDoList(driver);
            if (items != null)
            {
                foreach (IWebElement item in items)
                {
                    if (item.Text == itemName)
                    {
                        IWebElement itemCheckbox = item.FindElement(By.XPath(".//input[@type='checkbox']"));
                        return itemCheckbox;
                    }
                }
                return null;

            }
            else
            {
                return null;
            }
        }


        //Apply the filter
        public static void applyFilter(RemoteWebDriver driver, ItemSatus itemStatus)
        {
            IList<IWebElement> filters = driver.FindElementById("filters").FindElements(By.TagName("li"));
            if (filters != null)
            {
                foreach (IWebElement filter in filters)
                {
                    if (filter.Text == itemStatus.ToString())
                    {
                        filter.Click();
                        WaitForLoad(driver);
                        return;
                    }
                }
            }
        }


        [TestInitialize()]
        public void initialize()
        {
            var appSettings = ConfigurationManager.AppSettings;
            browser = appSettings["Browser"] ?? "Not Found";
            baseURL = appSettings["URL"] ?? "Not Found";
            if (browser == "Chrome")
            {
                driver = new ChromeDriver();
            }
            //wont work in IE browser, the link is not a https://
            else if (browser == "IE")
            {
                driver = new InternetExplorerDriver();
            }
            //can not start firefox browser, sounds like a selenium issu-https://github.com/mozilla/geckodriver/issues/539
            else if (browser == "Firefox")
            {
                driver = new FirefoxDriver();
            }
            else
            {
                throw new System.ArgumentException("is not [Chrome, IE, Firefox].", "Configure-Browser");
            }
            // driver.Url = baseURL;
        }

        
        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(1)]
        [Description("1. I want to add a To-do item")]
        public void verifyAddTodoItem()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string itemName = "Item1";
            addToDoItem(driver, itemName);
            Assert.AreEqual(verifyToDoItemExists(driver, itemName), true, itemName + "is Not added into the To-Do list");
            
        }


        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(2)]
        [Description("2. I want to edit the content of an existing To-do item")]
        public void verifyeEditTodoItem()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string itemName = "Item2";
            string itemNewName = "Item2 Updated";
            addToDoItem(driver, itemName);
            IWebElement item = getToDoItem(driver, itemName);
            if (item != null)
            {
                //Edit the to-do item's lable, can turn this into a method, but wont be resued in the other tests in this demo
                IWebElement itemLabel = item.FindElement(By.ClassName("ng-binding"));
                new Actions(driver).DoubleClick(itemLabel).Build().Perform();
                IWebElement itemLabeInput = item.FindElement(By.TagName("form")).FindElement(By.TagName("input"));
                itemLabeInput.Clear();
                itemLabeInput.SendKeys(itemNewName);
                itemLabeInput.SendKeys(Keys.Return);
            }

            Assert.AreEqual(verifyToDoItemExists(driver, itemNewName), true, itemName + " was not Edited, the new name should be "  + itemNewName);

        }

        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(3)]
        [Description("3. I can complete a To-do by clicking inside the circle UI to the left of the To-do")]
        public void verifCompletedTodoItem()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string itemName = "Item3";
            addToDoItem(driver, itemName);
            WaitForLoad(driver);
            IWebElement itemCheckbox = clickToDoItemCheckbox(driver, itemName);
            if (itemCheckbox != null)
            {
                Assert.AreEqual(itemCheckbox.Selected, true, "Item-"+itemName + " is Not set To Completed");
            }
            else
            {   Assert.AreEqual(itemCheckbox, null, "Item-" + itemName + " can Not be set To Completed");
            }
            

        }

        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(4)]
        [Description("4. I can re-activate a completed To-do by clicking inside the circle UI")]
        public void verifyReactiveTodoItem()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string itemName = "Item4";
            addToDoItem(driver, itemName);
            WaitForLoad(driver);
            IWebElement itemCheckbox = clickToDoItemCheckbox(driver, itemName);
            //Re-active a Completed To-do item
            if (itemCheckbox != null)
            {
                Assert.AreEqual(itemCheckbox.Selected, true, "Item-" + itemName + " is Not set To Completed");
                itemCheckbox = clickToDoItemCheckbox(driver, itemName);
                if (itemCheckbox != null)
                {
                    Assert.AreEqual(itemCheckbox.Selected, false, "Item-" + itemName + " is Not Reactived");
                }
            }
            else
            {
                Assert.AreEqual(itemCheckbox, null, "Item-" + itemName + " can Not be set To Completed/Reactived");
            }
        }

        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(5)]
        [Description("5. I can add a second To-do")]
        public void verifyAddSecondTodoItem()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string item1Name = "Item5_A";
            string item2Name = "Item5_B";
            addToDoItem(driver, item1Name);
            Assert.AreEqual(verifyToDoItemExists(driver, item1Name), true, item1Name + "is Not added into the To-Do list");
            addToDoItem(driver, item2Name);
            Assert.AreEqual(verifyToDoItemExists(driver, item2Name), true, item2Name + "is Not added into the To-Do list");
        }

        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(6)]
        [Description("6. I can complete all active To-dos by clicking the down arrow at the top-left of the UI")]
        public void verifyCompletedAllTodoItems()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string[] itemNames = { "Item6_A", "Item6_B", "Item6_C" };
            foreach(string itemName in itemNames)
            {
                addToDoItem(driver, itemName);
            }
            //Click Complete All down arrow 
            IWebElement selectAllButton = driver.FindElementById("toggle-all");
            selectAllButton.Click();
            WaitForLoad(driver);
            foreach (string itemName in itemNames)
            {
                IWebElement itemCheckbox = getToDoItemCheckBox(driver, itemName);
                if (itemCheckbox != null)
                {
                    Assert.AreEqual(itemCheckbox.Selected, true, "Item-" + itemName + " is Not set To Completed");
                }
                else
                {
                    Assert.AreEqual(itemCheckbox, null, "Item-" + itemName + " can Not be set To Completed");
                }
            }
        }

        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(7)]
        [Description("7. I can filter the visible To-dos by Completed state")]
        public void verifyFilteredCompletdTodoItems()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string[] itemNames = { "Item7_A", "Item7_B", "Item7_C" };
            foreach (string itemName in itemNames)
            {
                addToDoItem(driver, itemName);
            }
            //Set Item7_A and Item7_C to Completed
            IWebElement itemCheckbox = clickToDoItemCheckbox(driver, itemNames[0]);
            Assert.AreEqual(itemCheckbox.Selected, true, "Item-" + itemNames[0] + " is Not set To Completed");
            itemCheckbox = clickToDoItemCheckbox(driver, itemNames[2]);
            Assert.AreEqual(itemCheckbox.Selected, true, "Item-" + itemNames[2] + " is Not set To Completed");
            applyFilter(driver, ItemSatus.Completed);
            //Verify Item7_A and Item7_C are in the list, but not Item7_B
            Assert.AreEqual(verifyToDoItemExists(driver, itemNames[0]), true, itemNames[0] + "is Not in the Completed To-Do list");
            Assert.AreEqual(verifyToDoItemExists(driver, itemNames[2]), true, itemNames[2] + "is Not in the Completed To-Do list");
            Assert.AreEqual(verifyToDoItemExists(driver, itemNames[1]), false, itemNames[1] + "is Not Completed, but still in the Completed To-Do list");
        }

        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(8)]
        [Description("8. I can clear a single To-do item from the list completely by clicking the Close icon")]
        public void verifyClearSingleCompletdTodoItem()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            //Add Single Item
            string itemName = "Item8";
            addToDoItem(driver, itemName);
            Assert.AreEqual(verifyToDoItemExists(driver, itemName), true, itemName + "is Not added into the To-Do list");
            //Click the Close button
            getToDoItem(driver, itemName).Click();
            getToDoItem(driver, itemName).FindElement(By.TagName("button")).Click();
            WaitForLoad(driver);
            Assert.AreEqual(verifyToDoItemExists(driver, itemName), false, itemName + "is Not cleared from the To-Do list");
        }

        [TestMethod]
        [TestCategory("Selenium")]
        [Priority(9)]
        [Description("9. I can clear all completed To-do items from the list completely")]
        public void verifyClearAllCompletdTodoItems()
        {
            openPage(driver, baseURL, "AngularJS");
            WaitForLoad(driver);
            string[] itemNames = { "Item9_A", "Item9_B", "Item9_C" };
            foreach (string itemName in itemNames)
            {
                addToDoItem(driver, itemName);
                Assert.AreEqual(verifyToDoItemExists(driver, itemName), true, itemName + "is Not added into the To-Do list");
            }
            //Set all to-do items to Completed
            IWebElement selectAllButton = driver.FindElementById("toggle-all");
            selectAllButton.Click();
            WaitForLoad(driver);
            //Click ClearAll Button
            IWebElement clearAllButton = driver.FindElementById("clear-completed");
            clearAllButton.Click();
            WaitForLoad(driver);
            Assert.AreEqual(getToDoList(driver), null, "The Completed Items are Not cleared completely");
        }



        [TestCleanup()]
        public void tearDown()
        {
            driver.Quit();
        }

       
    }
}
