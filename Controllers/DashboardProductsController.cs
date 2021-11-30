using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ProjectInter.Models;
using ProjectInter.Data.Interfaces;
using System;
using System.IO;
using System.Text.Json;

namespace ProjectInter.Controllers
{
    public class DashboardProductsController : Controller
    {
        private IProductsRepository repositoryProducts;
        private IPurchaseRepository repositoryPurchase;

        public DashboardProductsController(IProductsRepository repositoryProducts, IPurchaseRepository repositoryPurchase)
        {
            this.repositoryProducts = repositoryProducts;
            this.repositoryPurchase = repositoryPurchase;
        }

        private List<Products> GetProductsFromView(){
            List<Products> products = repositoryProducts.GetAllProducts();
            return products;
        }
        [HttpGet]
        public ActionResult NewOrder()
        {
            return View(GetProductsFromView());
        }

        [HttpGet]
        public ActionResult Inventory()
        {
            List<Products> products = repositoryProducts.GetAllProducts();
            return View(products);
        }
        [HttpPost]
        public ActionResult Inventory(int idProduct, string name, double amount, string price, double inventory)
        {
            if (name != null && price != null)
                repositoryProducts.UpdateProduct(idProduct, name, Convert.ToDouble(price));
            else
                repositoryPurchase.Create(amount, idProduct, inventory);

            return RedirectToAction("Inventory");
        }
        [HttpGet]
        public ActionResult NewProduct()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewProduct(Products products)
        {

            IFormFile file = products.ImageFile;
            if (products.ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath)){
                   products.ImageFile.CopyTo(stream);
                }
                products.Image = fileName;
            }

            repositoryProducts.Create(products);
            return RedirectToAction("Inventory");
        }
        [HttpGet]
        public ActionResult DeleteProduct(int idProduct)
        {
            repositoryProducts.Delete(idProduct);
            return RedirectToAction("Inventory");
        }

        [HttpPost]
        public ActionResult NewOrder(CartProduct cart)
        {
            // Console.WriteLine(cart.IdPrimary);
            // foreach (var item in cart.IdAdd)
            // {
            //     Console.WriteLine(item);
            // }
            string carrinho = HttpContext.Session.GetString("carrinho");

            List<CartProduct> itens = null;

            if(carrinho == null)
            {
                itens = new List<CartProduct>();
            }
            else
            {
                itens = JsonSerializer.Deserialize<List<CartProduct>>(carrinho);
            }

            itens.Add(cart);

            string jsonString = JsonSerializer.Serialize(itens);

            HttpContext.Session.SetString("carrinho", jsonString);

            return RedirectToAction("NewOrder");
        }
    }
}
