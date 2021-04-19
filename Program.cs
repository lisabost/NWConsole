using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Edit a Category");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");

                    if (choice == "1")
                    {
                        var db = new NWConsole_96_LMBContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        logger.Info("User choice: 2 - Enter new Category");
                        try
                        {
                            var db = new NWConsole_96_LMBContext();
                            Categories category = InputCategory(db);
                            if (category != null)
                            {
                                db.AddCategory(category);
                                logger.Info("Category added - {name}", category.CategoryName);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                        }
                    }
                    else if (choice == "3")
                    {
                        var db = new NWConsole_96_LMBContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");

                        Categories category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");

                        if (category.Products.Count() != 0)
                        {
                            //Do not display discontinued products when displaying categories and related products
                            foreach (Products p in category.Products.Where(p => !p.Discontinued))
                            {
                                Console.WriteLine(p.ProductName);
                            }
                        }
                        else
                        {
                            //nice display message if there are no products in that category
                            Console.WriteLine("There are no products in this category.");
                        }
                    }
                    else if (choice == "4")
                    {
                        logger.Info("User choice: 4 - Display all categories and related active products.");
                        var db = new NWConsole_96_LMBContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            //do not display discontinued products
                            foreach (Products p in item.Products.Where(p => !p.Discontinued))
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }
                    else if (choice == "5")
                    {
                        logger.Info("User choice: 5 - Edit Category");
                        //edit category
                        Console.WriteLine("Choose the category to edit:");
                        var db = new NWConsole_96_LMBContext();
                        var category = GetCategory(db);
                        if (category != null)
                        {
                            //input new category
                            Categories UpdatedCategory = InputCategory(db);
                            if (UpdatedCategory != null)
                            {
                                UpdatedCategory.CategoryId = category.CategoryId;
                                db.EditCategory(UpdatedCategory);
                                logger.Info($"Category (id: {category.CategoryId}) updated.");
                            }
                        }
                    }
                    else if (choice == "6")
                    {
                        logger.Info("User choice: 6 - Delete Category");
                        //todo: delete category
                    }
                    Console.WriteLine();

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        public static Categories InputCategory(NWConsole_96_LMBContext db)
        {
            Categories category = new Categories();
            Console.WriteLine("Enter Category name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter Category description");
            category.Description = Console.ReadLine();

            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }
            return category;
        }

        public static Categories GetCategory(NWConsole_96_LMBContext db)
        {
            //display all categories
            var categories = db.Categories.OrderBy(c => c.CategoryId);
            foreach (Categories c in categories)
            {
                Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
            }
            if (int.TryParse(Console.ReadLine(), out int CategoryID))
            {
                Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryID);
                if (category != null)
                {
                    return category;
                }
            }
            logger.Error("Invalid Category Id");
            return null;
        }
    }
}
