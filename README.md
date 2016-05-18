# netQuery
netQuery.cs is a .NET class (C#) that allows you to get HtmlElement objects from the HtmlDocument object of a .NET webbrowser component with css selectors.
.NET HTML parsers are pretty cool but when you have to crawl a website by simulating a user navigation (sometimes there is no other way) you need to trigger events on some HtmlElement objects of the webbrowser HtmlDocument. Until now there was no way to get these HtmlElements objects with css selectors. Main css selectors are implemented. Check the source.

## Example

```c#

netQuery nq;
WebBrowser w = new WebBrowser();

w.Url = new Uri(someUri);
waitForPage(); // implement your own method to ensure w.document is not null and DOM is ready
nq = new netQuery(w.Document);

// typical crawling scenario

// login
nq.getElement("[name=login]").SetAttribute("value", "arsenelupin");
nq.getElement("[name=pwd]").SetAttribute("value", "gentleman");
nq.getElement(".sub").InvokeMember("click"); // submit
waitForPage();
nq.refresh();

// access search page
nq.getElement("ul.menu>li:eq(3)").InvokeMember("click");
waitForPage();
nq.refresh();

// fill Form
nq.getElement("[name=criterion1]").SetAttribute("value", val1);
nq.getElement("[name=criterion2]").SetAttribute("value", val2);
// ...
nq.getElement("button[type=submit]").InvokeMember("click");
waitForPage();
nq.refresh();

// get data
string data1 = nq.getElement("table.result>tr:eq(0)>td:eq(1)").InnerText;
string data2 = nq.getElement("table.result>tr:eq(0)>td:eq(2)").InnerText;
// ...

```
