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
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1) Categories");
                Console.WriteLine("2) Products");
                string option = Console.ReadLine();

                if (option == "1")
                {
                    string choice;
                    do
                    {
                        Console.WriteLine("1) Display Categories");
                        Console.WriteLine("2) Add Category");
                        Console.WriteLine("3) Display Category and related products");
                        Console.WriteLine("4) Display all Categories and their related products");
                        Console.WriteLine("5) Edit a Category");
                        Console.WriteLine("6) Delete a Category");
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
                                Categories validCategory = ValidateCategoryName(db, category);
                                if (validCategory != null)
                                {
                                    db.AddCategory(validCategory);
                                    logger.Info("Category added - {name}", validCategory.CategoryName);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                            }
                        }
                        else if (choice == "3")
                        {
                            logger.Info("User choice: 3 - Display category and related products");
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
                                UpdatedCategory.CategoryId = category.CategoryId;
                                Categories ValidUpdatedCategory = ValidateCategoryName(db, UpdatedCategory);
                                if (ValidUpdatedCategory != null)
                                {
                                    db.EditCategory(ValidUpdatedCategory);
                                    logger.Info($"Category (id: {category.CategoryId}) updated.");
                                }
                            }
                        }
                        else if (choice == "6")
                        {
                            logger.Info("User choice: 6 - Delete Category");
                            Console.WriteLine("Choose the category to delete");
                            var db = new NWConsole_96_LMBContext();
                            var category = GetCategory(db);
                            if (category != null)
                            {
                                //check to see if the category has dependencies
                                var products = db.Products.Where(p => p.CategoryId == category.CategoryId);

                                if (products.Count() == 0)
                                {
                                    //delete category
                                    db.DeleteCategory(category);
                                    logger.Info($"Category (id: {category.CategoryId}) deleted.");
                                }
                                else
                                {
                                    logger.Error("Cannot delete category with products in it. To delete the category, first remove any products.");
                                }
                            }
                        }
                        Console.WriteLine();

                    } while (choice.ToLower() != "q");
                }
                else if (option == "2")
                {
                    //Products time
                    logger.Info("User choice: 2 - Products");

                    string choice;
                    do
                    {
                        Console.WriteLine("1) Display Products");
                        Console.WriteLine("2) Add Product");
                        Console.WriteLine("3) Edit Product");
                        Console.WriteLine("4) Delete Product");
                        Console.WriteLine("\"q\" to quit");
                        choice = Console.ReadLine();
                        Console.Clear();
                        logger.Info($"Option {choice} selected");

                        if (choice == "1")
                        {
                            logger.Info("User choice: 1 - Display Products");
                            var db = new NWConsole_96_LMBContext();

                            Console.WriteLine("1) Display Active Products");
                            Console.WriteLine("2) Display Discontinued Products");
                            Console.WriteLine("3) Display All Products");
                            Console.WriteLine("4) Display Specific Product Information");
                            string input = Console.ReadLine();

                            if (input == "1")
                            {
                                logger.Info("User choice: 1 - Display active products");
                                //display active products
                                var activeQuery = db.Products.OrderBy(p => p.ProductName).Where(p => !p.Discontinued);
                                Console.WriteLine($"Number of Active Products: {activeQuery.Count()}");

                                if (activeQuery.Count() != 0)
                                {
                                    foreach (var product in activeQuery)
                                    {
                                        Console.WriteLine(product.ProductName);
                                    }
                                    Console.WriteLine();
                                }
                                else
                                {
                                    logger.Info("No active products");
                                    Console.WriteLine("There are no active products");
                                }
                            }
                            else if (input == "2")
                            {
                                logger.Info("User choice: 2 - Display discontinued prodcuts");
                                //display discontinuted products
                                var discontinuedQuery = db.Products.OrderBy(p => p.ProductName).Where(p => p.Discontinued);
                                Console.WriteLine($"Number of Discontinued Products: {discontinuedQuery.Count()}");

                                Console.ForegroundColor = ConsoleColor.Red;
                                if (discontinuedQuery.Count() != 0)
                                {
                                    foreach (var product in discontinuedQuery)
                                    {
                                        Console.WriteLine(product.ProductName);
                                    }
                                    Console.WriteLine();
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    logger.Info("No discontinued products");
                                }
                            }
                            else if (input == "3")
                            {
                                logger.Info("User choice: 3 - Display all products");
                                //display all products - active and discontinued
                                var activeQuery = db.Products.OrderBy(p => p.ProductName).Where(p => !p.Discontinued);
                                var discontinuedQuery = db.Products.OrderBy(p => p.ProductName).Where(p => p.Discontinued);

                                Console.WriteLine("Active Products:");
                                foreach (var product in activeQuery)
                                {
                                    Console.WriteLine($"\t{product.ProductName}");
                                }
                                Console.WriteLine($"Total Active Products: {activeQuery.Count()}");

                                Console.WriteLine("Discontinued Products:");
                                foreach (var product in discontinuedQuery)
                                {
                                    Console.WriteLine($"\t{product.ProductName}");
                                }
                                Console.WriteLine($"Total Discontinued Products: {discontinuedQuery.Count()}");

                            }
                            else if (input == "4")
                            {
                                logger.Info("User choice 4: Display specific product information");
                                Console.WriteLine("Choose a product to display");
                                var product = GetProduct(db);

                                if (product != null)
                                {
                                    var isActive = product.Discontinued;
                                    string status;
                                    if (isActive)
                                    {
                                        status = "true";
                                    }
                                    else
                                    {
                                        status = "false";
                                    }
                                    Console.WriteLine($"Product Id: {product.ProductId}\nProduct name: {product.ProductName}\nSupplier Id: {product.SupplierId}\nCategory Id: {product.CategoryId}\nQuantity Per Unit: {product.QuantityPerUnit}\nUnit Price: {product.UnitPrice:C2}\nUnits in Stock: {product.UnitsInStock}\nUnits on Order: {product.UnitsOnOrder}\nReorder Level: {product.ReorderLevel}\nDiscontinued: {status}\n");
                                }
                                else
                                {
                                    logger.Error("No product to display");
                                }
                            }
                            else
                            {
                                logger.Error("Invalid Choice");
                            }
                        }
                        else if (choice == "2")
                        {
                            logger.Info("User choice: 2 - Add product");
                            //add product - products have name (string, not null), supplier Id (int), category Id (int), quantity per unit (string), unit price (double), 
                            //units in stock (smallint), units on order (smallint), reorder level (smallint), discontinued (bit, not null)
                            try
                            {
                                var db = new NWConsole_96_LMBContext();
                                Products product = InputProduct(db);
                                Products validProduct = ValidateProductName(db, product);
                                if (validProduct != null)
                                {
                                    db.AddProduct(validProduct);
                                    logger.Info("Product added - {name}", validProduct.ProductName);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                            }
                        }
                        else if (choice == "3")
                        {
                            logger.Info("User choice: 3 - Edit product");
                            //edit product
                            Console.WriteLine("Choose a product to edit");
                            var db = new NWConsole_96_LMBContext();
                            var product = GetProduct(db);
                            if (product != null)
                            {
                                Products UpdatedProduct = InputProduct(db);
                                UpdatedProduct.ProductId = product.ProductId;
                                Products ValidUpdatedProduct = ValidateProductName(db, UpdatedProduct);
                                if (ValidUpdatedProduct != null)
                                {
                                    db.EditProduct(ValidUpdatedProduct);
                                    logger.Info($"Product (id: {product.ProductId}) updated.");
                                }
                            }
                        }
                        else if (choice == "4")
                        {
                            logger.Info("User choice: 4 - Delete product");
                            Console.WriteLine("Choose a product to delete");
                            var db = new NWConsole_96_LMBContext();
                            var product = GetProduct(db);

                            if (product != null)
                            {
                                //check for dependencies
                                var orderDetails = db.OrderDetails.Where(o => o.ProductId == product.ProductId);
                                if (orderDetails.Count() != 0)
                                {
                                    //if there are order details attached we can't delete the product
                                    logger.Error("Cannot delete product where order exists. Mark as discontinued instead.");
                                }
                                else
                                {
                                    db.DeleteProduct(product);
                                    logger.Info($"Product (id: {product.ProductId}) deleted.");

                                }
                            }
                        }
                    }
                    while (choice.ToLower() != "q");
                }
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
            return category;
        }

        public static Products InputProduct(NWConsole_96_LMBContext db)
        {
            //add product - products have name (string, not null), supplier Id (int), category Id (int), quantity per unit (string), unit price (double), 
            //units in stock (smallint), units on order (smallint), reorder level (smallint), discontinued (bit, not null)
            Products product = new Products();
            Console.WriteLine("Enter product name:");
            product.ProductName = Console.ReadLine();

            Console.WriteLine("Is the product active: (y/n)");
            string active = Console.ReadLine();
            if (active.ToLower() == "y")
            {
                product.Discontinued = false;
            }
            else if (active.ToLower() == "n")
            {
                product.Discontinued = true;
            }
            else
            {
                logger.Error("Invalid input");
            }

            Console.WriteLine("Enter the supplier Id:");
            var supplierID = Console.ReadLine();
            if (supplierID == null || supplierID == "")
            {
                logger.Info("No supplier Id entered");
                product.SupplierId = null;
            }
            else
            {
                product.SupplierId = int.Parse(supplierID);
            }

            Console.WriteLine("Enter the category Id:");
            var categoryID = Console.ReadLine();
            if (categoryID == null || categoryID == "")
            {
                logger.Info("No category Id entered");
                product.CategoryId = null;
            }
            else
            {
                product.CategoryId = int.Parse(categoryID);
            }

            Console.WriteLine("Enter quantity per unit:");
            var quantityPerUnit = Console.ReadLine();
            if (quantityPerUnit == null || quantityPerUnit == "")
            {
                logger.Info("No quantity per unit entered");
                product.QuantityPerUnit = null;
            }
            else
            {
                product.QuantityPerUnit = quantityPerUnit;
            }

            Console.WriteLine("Enter unit price:");
            var unitPrice = Console.ReadLine();
            if (unitPrice == null || unitPrice == "")
            {
                logger.Info("No unit price entered");
                product.UnitPrice = null;
            }
            else
            {
                product.UnitPrice = decimal.Parse(unitPrice);
            }

            Console.WriteLine("Enter units in stock:");
            var unitsInStock = Console.ReadLine();
            if (unitsInStock == null || unitsInStock == "")
            {
                logger.Info("No units in stock entered");
                product.UnitsInStock = null;
            }
            else
            {
                product.UnitsInStock = short.Parse(unitsInStock);
            }

            Console.WriteLine("Enter units on order:");
            var unitsOnOrder = Console.ReadLine();
            if (unitsOnOrder == null || unitsOnOrder == "")
            {
                logger.Info("No units on order entered");
                product.UnitsOnOrder = null;
            }
            else
            {
                product.UnitsOnOrder = short.Parse(unitsOnOrder);
            }

            Console.WriteLine("Enter reorder level:");
            var reorderLevel = Console.ReadLine();
            if (reorderLevel == null || reorderLevel == "")
            {
                logger.Info("No reorder level entered");
                product.ReorderLevel = null;
            }
            else
            {
                product.ReorderLevel = short.Parse(reorderLevel);
            }
            return product;
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

        public static Products GetProduct(NWConsole_96_LMBContext db)
        {
            //display all products
            var products = db.Products.OrderBy(p => p.ProductId);
            foreach (Products p in products)
            {
                Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }
            if (int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Products product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
                if (product != null)
                {
                    return product;
                }
            }
            logger.Error("Invalid Product Id");
            return null;
        }

        public static Products ValidateProductName(NWConsole_96_LMBContext db, Products product)
        {
            //if we are editing the product but not changing the name, it is ok
            var duplicateProduct = db.Products.Where(p => p.ProductName == product.ProductName).FirstOrDefault();
            if (duplicateProduct.ProductId == product.ProductId)
            {
                return product;
            }
            else
            {
                ValidationContext context = new ValidationContext(product, null, null);
                List<ValidationResult> results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(product, context, results, true);
                if (isValid)
                {
                    // check for unique name
                    if (db.Products.Any(p => p.ProductName == product.ProductName))
                    {
                        // generate validation error
                        isValid = false;
                        results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
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
                return product;
            }
        }

        public static Categories ValidateCategoryName(NWConsole_96_LMBContext db, Categories category)
        {
            //if we are editing the product but not changing the name, it is ok
            var duplicateCategory = db.Categories.Where(c => c.CategoryName == category.CategoryName).FirstOrDefault();
            if (duplicateCategory.CategoryId == category.CategoryId)
            {
                return category;
            }
            else
            {
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
        }
    }
}
