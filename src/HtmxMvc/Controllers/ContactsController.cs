using HtmxMvc.Models;
using HtmxMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace HtmxMvc.Controllers;

[AutoValidateAntiforgeryToken]
public sealed class ContactsController : Controller
{
    private readonly ContactService _service;

    public ContactsController(ContactService service) => _service = service;

    [HttpGet("/")]
    public IActionResult Index()
    {
        return View(_service.GetAll());
    }

    [HttpGet("/contacts/list")]
    public IActionResult List(string? q)
    {
        return PartialView("_ContactList", _service.Search(q));
    }

    [HttpPost("/contacts")]
    public IActionResult Create(Contact contact)
    {
        if (!ModelState.IsValid) return BadRequest();
        var created = _service.Add(new Contact
        {
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone
        });
        return PartialView("_ContactRow", created);
    }

    [HttpGet("/contacts/{id:int}/edit")]
    public IActionResult Edit(int id)
    {
        var c = _service.Get(id);
        return c is null ? NotFound() : PartialView("_ContactEditRow", c);
    }

    [HttpPut("/contacts/{id:int}")]
    public IActionResult Update(int id, Contact contact)
    {
        if (!ModelState.IsValid) return BadRequest();
        var updated = _service.Update(id, contact);
        return updated is null ? NotFound() : PartialView("_ContactRow", updated);
    }

    [HttpGet("/contacts/{id:int}")]
    public IActionResult Row(int id)
    {
        var c = _service.Get(id);
        return c is null ? NotFound() : PartialView("_ContactRow", c);
    }

    [HttpDelete("/contacts/{id:int}")]
    public IActionResult Delete(int id)
    {
        return _service.Delete(id) ? Ok() : NotFound();
    }
}
