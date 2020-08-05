using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using CsvHelper;
using System.Text;

namespace TAMUScraper
{
	class Program
	{
		static void Main(string[] args)
		{
			List<string> final = new List<string>();
			var ProfileLinks = new List<string>();
			var names = new List<string>();
			var emails = new List<string>();
			var stronk = new List<string>();
			var dept = new List<string>();

			for (char c = 'A'; c <= 'Z'; c++)
			{
				c.ToString();
				HtmlWeb web = new HtmlWeb();
				//You can change the link below to change search criteria. The "c" variable is the alphabet loop, so make sure it is always after "giveName="
				HtmlDocument doc = web.Load("https://gateway.tamu.edu/directory-search/?branch=people&amp;givenName="+c+ "&amp;sn=&amp;cn=&amp;title=Assistant+Dean&amp;tamuEduPersonDepartmentName=&amp;tamuEduPersonMember=&amp;tamuEduPersonClassification=&amp;tamuEduPersonMajor=");

				AddLinks(doc, ProfileLinks);

				foreach (var item in ProfileLinks)
				{
					string services = "https://gateway.tamu.edu";
					string path = services + item;
					//Console.WriteLine(path);
					HtmlWeb web2 = new HtmlWeb();
					HtmlDocument doc2 = web2.Load(path);

					//Grabs email address
					try
					{
						var HeaderNames = doc2.DocumentNode
						.SelectNodes(".//a").ToList();

						//This one is a little weird. Not every contact in the directory has an email, so if I used the same foreach loop it wouldn't pull info from contacts without emails
						//So instead it is a for loop and finds the email which is always at index 5. However if they don't have an email it will still pull something so the list can still be matched uo
						//Then, to avoid massive amounts of duplicates, I only add the emails to the email list when the for loop is about to break.
						for (int i = 0; i < HeaderNames.Count; i++)
						{
							HtmlNode item2 = HeaderNames[5];

							if (i == HeaderNames.Count - 1)
								emails.Add(item2.InnerText.ToString());
							
						}
					}
					catch (SystemException e)
					{
						Console.WriteLine(e);
					}
					
					//Grabs Name
					try
					{
						var HeaderNames = doc2.DocumentNode
						.SelectNodes(".//h2").ToList();

						foreach (var item2 in HeaderNames)
						{
							names.Add(item2.InnerText.ToString());
						}
					}
					catch (SystemException e)
					{
						Console.WriteLine(e);
					}

					//Grabs Title, I called it STRONK because the title is within the strong tag
					try
					{
						var HeaderNames = doc2.DocumentNode
						.SelectNodes(".//strong").ToList();

						foreach (var item2 in HeaderNames)
						{
							if (item2.InnerText.ToString() != "Information")
								stronk.Add(item2.InnerText.ToString());
						}
					}
					catch (SystemException e)
					{
						Console.WriteLine(e);
					}

					//Grabs department
					try
					{
						var HeaderNames = doc2.DocumentNode
						.SelectNodes(".//li").ToList();

						foreach (var item2 in HeaderNames)
						{
							if (item2.InnerText.ToString().StartsWith("Department"))
								dept.Add(item2.InnerText.ToString());
						}
					}
					catch (SystemException e)
					{
						Console.WriteLine(e);
					}
				}

				//This takes names, titles, emails, and department and combines them into a final list.
				for (int i = 0; i < names.Count; i++)
				{
					final.Add(names.ElementAt(i) + "|" + stronk.ElementAt(i) + "|" + emails.ElementAt(i) + "|" + dept.ElementAt(i));
				}

				//De-dupe the list because for some reason the scraper grabs duplicates of every line
				final = final.Distinct().ToList();
			}

			Finish(final);
		}

		private static void Finish(List<string> final)
		{
			File.WriteAllLines(@"C:\Users\jhiney\Desktop\deans.csv", final);
		}

		//This method scrapes the aggregagte search before the secondary scraper goes into individual profiles
		private static void AddLinks(HtmlDocument doc, List<string> ProfileLinks)
		{
			foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
			{
				// Get the value of the HREF attribute
				string hrefValue = link.GetAttributeValue("href", string.Empty);

				if (hrefValue.StartsWith("/directory-search/people/"))
				{
					ProfileLinks.Add(hrefValue);
				}
			}
		}


	}
}
