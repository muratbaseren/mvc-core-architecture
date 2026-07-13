using App.Modules.Products.Application.Commands;
using App.Modules.Products.Application.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.Products.Controllers;

/// <summary>
/// Ürün CRUD ekranları. Tüm iş mantığı MediatR üzerinden yürür;
/// controller yalnızca HTTP/görünüm katmanıdır.
/// </summary>
public class ProductsController(ISender sender) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await sender.Send(new GetProductsQuery(), cancellationToken);
        return View(products);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create() => View(new CreateProductCommand("", null, 0, 0));

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(command, cancellationToken);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Ürün eklendi.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, result.Error!);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return View(command);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var product = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
        if (product is null)
            return NotFound();

        var model = new UpdateProductCommand(
            product.Id, product.Name, product.Description,
            product.Price, product.Stock, product.IsActive);
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(command, cancellationToken);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Ürün güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, result.Error!);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return View(command);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] =
            result.IsSuccess ? "Ürün silindi." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
