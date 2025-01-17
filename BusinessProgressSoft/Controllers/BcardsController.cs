﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessProgressSoft.Models;
using System.Xml.Serialization;
using CsvHelper;
using System.Globalization;
using BusinessProgressSoft.Models.Services;
using System.Xml.Linq;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;

namespace BusinessProgressSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BcardsController : ControllerBase
    {
        private readonly BusinessProgressSoftContext _context;
        private readonly ICSVService _csvService;
        private readonly ICards _cards;
        private readonly FileExtensionContentTypeProvider _provider;

        public BcardsController(BusinessProgressSoftContext context, ICSVService cSV, ICards cards)
        {
            _context = context;
            _csvService = cSV;
            _cards = cards;
            _provider = new FileExtensionContentTypeProvider();
        }

        //[HttpGet("GetCard")]
        //public IActionResult GetBcards()
        //{
        //    var cards = _cards.GetCards();
        //    return Ok(cards); 
        //}
        [HttpGet("GetCard")]
        public IActionResult GetBcards()
        {
            var cards = _cards.GetCards();

            // Iterate through each card and decode the photo if it exists
            foreach (var card in cards)
            {
                if (!string.IsNullOrEmpty(card.Photo))
                {
                    // Convert the Base64 string to a data URL format, assuming it's already stored as Base64 in the database
                    byte[] data = Convert.FromBase64String(card.Photo);
                    card.Photo = Encoding.UTF8.GetString(data);
                    string imageName = Path.GetFileName(card.Photo);
                    card.Photo = imageName;
                }
            }

            return Ok(cards);
        }

        [HttpGet("getImage/{imagePath}")]
        public IActionResult GetImage(string imagePath)
        {
           // string imageName = Path.GetFileName(imagePath);
            //var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");

            //var fullPath = Path.Combine(folderPath, imageName);
            if (!System.IO.File.Exists(imagePath)) return NotFound();
            if (!_provider.TryGetContentType(imagePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var imageFileStream = System.IO.File.OpenRead(imagePath);

            return File(imageFileStream,contentType);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<ActionResult<Bcard>> GetBcard(int id)
        {
          if (_context.Bcards == null)
          {
              return NotFound();
          }
            var bcard = await _context.Bcards.FindAsync(id);

            if (bcard == null)
            {
                return NotFound();
            }

            return bcard;
        }


        [HttpPut]
        [Route("UpdateCard")]

        public async Task<IActionResult> PutBcard(Bcard bcard)
        {
            if ( bcard == null)
            {
                return BadRequest();
            }

            _context.Entry(bcard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BcardExists(bcard.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost("GetFromCSV")]
        public async Task<IActionResult> GetFromCSV([FromForm] IFormFileCollection file)
        {
            var card = _csvService.ReadCSV<Bcard>(file[0].OpenReadStream());
            return Ok(card);
        }
      
        //[HttpPost("InsertCardFromCSV")]
        //public async Task<IActionResult> CreateCardFromCSV([FromForm] IFormFileCollection files)
        //{
        //    if (files == null || files.Count == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    var file = files[0];

        //    // Validate file type (must be CSV)
        //    if (file.ContentType != "text/csv")
        //    {
        //        return BadRequest("Invalid file type. Please upload a CSV file.");
        //    }

        //    try
        //    {
        //        // Parse the CSV file
        //        var records = _csvService.ReadCSV<Bcard>(file.OpenReadStream());

        //        // Add records to the context in batches for performance improvement
        //        _context.Bcards.AddRange(records);
        //        await _context.SaveChangesAsync();

        //        return Ok("Records successfully added.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception and return error response
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        [HttpPost("CreateCardFromCSV")]
        public async Task<IActionResult> CreateCardFromCSV()
        {
            var file = Request.Form.Files[0];
            if(file == null)
            {
                return BadRequest();
            }
            if (file.ContentType != "text/csv")
            {
                return BadRequest("Invalid file type. Please upload a CSV file.");
            }
            try
            {
                // Parse the CSV file
                var records = _csvService.ReadCSV<Bcard>(file.OpenReadStream());
                foreach (var card in records)
                {
                    if (!string.IsNullOrEmpty(card.Photo))
                    {
                        string imagePath = card.Photo;
                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imagePath);
                        var fullPath = Path.Combine("C:\\Users\\DELL\\source\\repos\\BusinessProgressSoftAngular\\src\\assets\\Images", fileName);


                        if (System.IO.File.Exists(imagePath))
                        {
                            // Save to the first path
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            //byte[] imageBytes = Encoding.UTF8.GetBytes(fullPath);
                            //card.Photo = Convert.ToBase64String(imageBytes); 
                            byte[] bytes = Encoding.UTF8.GetBytes(imagePath);
                            card.Photo = Convert.ToBase64String(bytes);
                        }
                        else
                        {
                            return BadRequest($"Image file not found: {imagePath}");
                        }
                    }
                }
                // Add records to the context in batches for performance improvement
                _context.Bcards.AddRange(records);
                await _context.SaveChangesAsync();

                return Ok("Records successfully added.");
            }
            catch (Exception ex)
            {
                // Log exception and return error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
       
        [HttpGet("ExportCardsToCSV")]
        public async Task<IActionResult> ExportCardToCSV()
        {
            var records = await _context.Bcards.ToListAsync();
            foreach (var card in records)
            {
                if (!string.IsNullOrEmpty(card.Photo))
                {
                    // Convert the Base64 string to a data URL format, assuming it's already stored as Base64 in the database
                    byte[] data = Convert.FromBase64String(card.Photo);
                    card.Photo = Encoding.UTF8.GetString(data);
                }
            }
            // Define the file path
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
            var fileName = Guid.NewGuid().ToString() + "-" + "Bcards.csv";
            var filePath = Path.Combine(folderPath, fileName);

            // Ensure the folder exists, create it if it doesn't
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Write the CSV file
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                _csvService.WriteCSV(stream, records);
            }

            // Serve the file to the user as a download
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "text/csv", fileName);
        }


        [HttpPost("InsertCardFromXML")]
        public async Task<IActionResult> CreateCardFromXML()
        {
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                List<Bcard> businessCards = new List<Bcard>();

                // Read the XML file from the uploaded stream
                using (var stream = file.OpenReadStream())
                {
                    XDocument doc = XDocument.Load(stream);

                    // Iterate through each "Bcard" element inside "DocumentElement"
                    foreach (XElement element in doc.Descendants("DocumentElement").Descendants("Bcard"))
                    {
                        Bcard bcard = new Bcard
                        {
                            Name = element.Element("Name").Value,
                            Gender = element.Element("Gender").Value,
                            Birth = DateTime.TryParse(element.Element("Birth").Value, out var birthDate) ? (DateTime?)birthDate : null,
                            Email = element.Element("Email").Value,
                            Phone = element.Element("Phone").Value,
                            Photo = element.Element("Photo").Value,
                            Address = element.Element("Address").Value
                        };

                        businessCards.Add(bcard);
                    }
                }

                foreach (var card in businessCards)
                {
                    string imagePath = card.Photo;
                    var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imagePath);
                    var fullPath = Path.Combine("C:\\Users\\DELL\\source\\repos\\BusinessProgressSoftAngular\\src\\assets\\Images", fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    //byte[] bytes = Encoding.UTF8.GetBytes(fullPath);
                    //card.Photo = Convert.ToBase64String(bytes);
                    byte[] bytes = Encoding.UTF8.GetBytes(imagePath);
                    card.Photo = Convert.ToBase64String(bytes);
                    _context.Bcards.Add(card);
                }

                await _context.SaveChangesAsync();

                return Ok("Business cards added successfully from XML.");
            }
            catch (Exception ex)
            {
                // Handle errors
                return BadRequest($"Error adding business cards: {ex.Message}");
            }
        }

        [HttpGet("ExportCardsToXML")]
        public async Task<IActionResult> ExportCardsToXML()
        {
            try
            {
                // Retrieve the list of Bcards from the database
                var businessCards = _context.Bcards.ToList();
                // Define the file path
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
                var fileName = Guid.NewGuid().ToString() + "-" + "Bcards.xml";
                var filePath = Path.Combine(folderPath, fileName);
                // Ensure the folder exists, create it if it doesn't
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using(FileStream stream = new FileStream(filePath,FileMode.Create,FileAccess.Write))
                {
                    // Create the XML structure
                    XDocument xmlDoc = new XDocument(
                        new XElement("DocumentElement",
                            businessCards.Select(b => new XElement("Bcard",
                                new XElement("Name", b.Name),
                                new XElement("Gender", b.Gender),
                                new XElement("Birth", b.Birth?.ToString("MM/dd/yyyy")),
                                new XElement("Email", b.Email),
                                new XElement("Phone", b.Phone),
                                new XElement("Photo", ConvertFromBase64(b.Photo)),
                                new XElement("Address", b.Address)
                            ))
                        )
                    );
                    xmlDoc.Save(stream);

                }


                //// Prepare the XML file for download
                //var stream = new MemoryStream();
                //stream.Position = 0; // Reset stream position for reading

                // Return the XML file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                return File(fileBytes, "application/xml", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error exporting business cards: {ex.Message}");
            }
        }

        private string ConvertFromBase64(string pathBase64)
        {
            byte[] bytes = Convert.FromBase64String(pathBase64);
            string pathName = Encoding.UTF8.GetString(bytes);
            return pathName;
        }

        [HttpPost("InsertCard")]
        public async Task<bool> CreateCard(Bcard card)
        {
            if(card == null)
            {
                return false;
            }
            _context.Bcards.Add(card);
            await _context.SaveChangesAsync();
            return true;
        }

        [HttpDelete]
        [Route("DeleteCard/{id}")]
        public async Task<IActionResult> DeleteBcard(int id)
        {
            if (_context.Bcards == null)
            {
                return NotFound();
            }
            var bcard = await _context.Bcards.FindAsync(id);
            if (bcard == null)
            {
                return NotFound();
            }

            _context.Bcards.Remove(bcard);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool BcardExists(int id)
        {
            return (_context.Bcards?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet("FilterByEmail/{email}")]
        public async Task<ActionResult<Bcard>> Filter(string email)
        {
            var card = await _context.Bcards.FirstOrDefaultAsync(c => c.Email.Contains(email));
            return Ok(card);
        }
        
        //[Route("uploadImage")]

        //[HttpPost]
        //public Bcard UploadImage()
        //{
        //    var file = Request.Form.Files[0];
        //    var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        //    var fullPath = Path.Combine("C:\\Users\\DELL\\source\\repos\\BusinessProgressSoftAngular\\src\\assets\\Images", fileName);
        //    using (var stream = new FileStream(fullPath, FileMode.Create))
        //    {
        //        file.CopyTo(stream);
        //    }
        //    Bcard card = new Bcard();
        //    card.Photo = fileName;
        //    return card;
        //}

        [Route("uploadImage")]
        [HttpPost]
        public Bcard UploadImage()
        {
            var file = Request.Form.Files[0];
            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");

            var firstPath = Path.Combine("C:\\Users\\DELL\\source\\repos\\BusinessProgressSoftAngular\\src\\assets\\Images", fileName);

            var secondPath = Path.Combine(folderPath, fileName);

            // Save to the first path
            using (var stream = new FileStream(firstPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Save to the second path
            using (var stream = new FileStream(secondPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            Bcard card = new Bcard();

            // Try to store photo base64
            card.Photo = Convert.ToBase64String(Encoding.UTF8.GetBytes(secondPath));
            //card.Photo = fileName;
            return card;
        }

    }
}
