using BusinessProgressSoft.Controllers;
using BusinessProgressSoft.Models;
using BusinessProgressSoft.Models.Services;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Moq;
using NuGet.ContentModel;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace BusnissCardsTest
{
    public class BusnissControllerTests
    {
        private readonly Mock<BusinessProgressSoftContext> _mockContext;
        private readonly Mock<DbSet<Bcard>> _mockSet;
        private readonly Mock<ICSVService> _mockCSVervice;
        private readonly BcardsController _controller;
        private readonly Mock<ICards> _mockCards;
        public BusnissControllerTests()
        {
            _mockContext = new Mock<BusinessProgressSoftContext>();
            _mockSet = new Mock<DbSet<Bcard>>();
            _mockCSVervice = new Mock<ICSVService>();
            _mockCards = new Mock<ICards>();
            _mockContext.Setup(m => m.Bcards).Returns(_mockSet.Object);
            _controller = new BcardsController(_mockContext.Object,_mockCSVervice.Object, _mockCards.Object);
        }

        //[Fact]
        //public void Method_Scenario_Outcome()
        //{
        // Arange
        // Act
        // Asset
        //}

        [Fact]
        public void GetCards_ReturnsOkResult_WithListOfCards()
        {
            // Arrange
            var cards = new List<Bcard>
            {
                new Bcard { Id = 1, Name = "Card 1" },
                new Bcard { Id = 2, Name = "Card 2" }
            };

            _mockCards.Setup(c => c.GetCards()).Returns(cards);

            // Act
            var result = _controller.GetBcards();

            var okResult = Assert.IsType<OkObjectResult>(result); 
            var responseList = Assert.IsAssignableFrom<IEnumerable<Bcard>>(okResult.Value); 

            // Assert
            Assert.NotNull(responseList); 
            Assert.Equal(2, responseList.Count()); 
            Assert.Equal("Card 1", responseList.ElementAt(0).Name); 
            Assert.Equal("Card 2", responseList.ElementAt(1).Name);
        }

        [Fact]
        public async Task InsertCard_AddNewCard_ReturnNewCard()
        {
            //Arrange
            var card = new Bcard()
            {
                Name = "Test",
                Gender = "t",
                Email = "test@t",
                Phone = "10111",
                Photo = "test.png",
                Birth = DateTime.Now,
                Address = "irbid"
            };
            
            // Act
            var result = await _controller.CreateCard(card);
            // Assert
            Assert.NotNull(result);
            Assert.True(result); 
        }
        [Fact]
        public async Task CreateCardFromCSV_ReturnsBadRequest_WhenFileIsInvalid()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var files = new FormFileCollection { fileMock.Object };
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Form = formCollection }
                }
            };

            fileMock.Setup(f => f.ContentType).Returns("text/plain"); // Invalid content type

            // Act
            var result = await _controller.CreateCardFromCSV();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateCardFromCSV_ReturnsOk_WhenFileIsValid()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var files = new FormFileCollection { fileMock.Object };
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Form = formCollection }
                }
            };

            fileMock.Setup(f => f.ContentType).Returns("text/csv");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            _mockCSVervice.Setup(s => s.ReadCSV<Bcard>(It.IsAny<Stream>())).Returns(new List<Bcard>());

            // Act
            var result = await _controller.CreateCardFromCSV();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateCardFromXML_ValidFile_ReturnsOkResult()
        {
            // Arrange
            var xmlContent = @"
    <DocumentElement>
        <Bcard>
            <Name>Test xUnit</Name>
            <Gender>M</Gender>
            <Birth>2024-01-01</Birth>
            <Email>test@test.com</Email>
            <Phone>1234567890</Phone>
            <Photo>photo</Photo>
            <Address>amman</Address>
        </Bcard>
    </DocumentElement>";

            var file = new FormFile(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlContent)), 0, xmlContent.Length, "file", "test.xml");

            // Mock the HttpContext and the Request
            var context = new DefaultHttpContext();
            var files = new FormFileCollection { file }; // Directly using FormFileCollection
            context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);
            _controller.ControllerContext.HttpContext = context;

            // Act
            var result = await _controller.CreateCardFromXML();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Business cards added successfully from XML.", okResult.Value);
            _mockSet.Verify(m => m.Add(It.IsAny<Bcard>()), Moq.Times.Once); // Verify the Add method was called once
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Moq.Times.Once); // Verify SaveChangesAsync was called once
        }

        [Fact]
        public async Task CreateCardFromXML_NoFile_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var files = new FormFileCollection { fileMock.Object };
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Form = formCollection }
                }
            };

            fileMock.Setup(f => f.ContentType).Returns("text/plain"); // Invalid content type

            // Act
            var result = await _controller.CreateCardFromXML();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateCardFromXML_InvalidXml_ReturnsBadRequest()
        {
            // Arrange
            var invalidXmlContent = @"<DocumentElement><Bcard><Name>Raneem</Name></Bcard>"; // Missing required elements
            var file = new FormFile(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidXmlContent)), 0, invalidXmlContent.Length, "file", "test.xml");

            var context = new DefaultHttpContext();
            var files = new FormFileCollection { file }; // Create collection with the invalid file
            context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);
            _controller.ControllerContext.HttpContext = context;

            // Act
            var result = await _controller.CreateCardFromXML();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Error adding business cards", (string)badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteCard_ReturnsNotFound_WhenCardsNull()
        {
            // Arrange
            var cardId = 1;
            Bcard nullCard = null;
            _mockSet.Setup(c => c.FindAsync(It.IsAny<int>())).ReturnsAsync(nullCard);
            // Act
            var result = await _controller.DeleteBcard(cardId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCard_ReturnsOk_WhenCardFound()
        {
            // Arrange
            var cardId = 1;
            var card = new Bcard { Id = cardId , Name = "raneem"};
            _mockSet.Setup(c => c.FindAsync(cardId)).ReturnsAsync(card);
            _mockSet.Setup(c => c.Remove(card));
            // Act
            var result = await _controller.DeleteBcard(cardId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ExportCardToCSV_ReturnsCSVFile()
        {
            // Arrange
            var records = new List<Bcard>()
            {
                new Bcard { Id = 1, Name = "raneem" },
                new Bcard { Id = 2, Name = "ahmad" }
            }.AsQueryable();

            // Mock the DbSet to support async operations
            var mockCardSet = new Mock<DbSet<Bcard>>();

            mockCardSet.As<IQueryable<Bcard>>()
                       .Setup(m => m.Provider)
                       .Returns(records.Provider);
            mockCardSet.As<IQueryable<Bcard>>()
                       .Setup(m => m.Expression)
                       .Returns(records.Expression);
            mockCardSet.As<IQueryable<Bcard>>()
                       .Setup(m => m.ElementType)
                       .Returns(records.ElementType);
            mockCardSet.As<IQueryable<Bcard>>()
                       .Setup(m => m.GetEnumerator())
                       .Returns(records.GetEnumerator());

            // Mock the async enumeration support
            mockCardSet.As<IAsyncEnumerable<Bcard>>()
                       .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                       .Returns(new TestAsyncEnumerator<Bcard>(records.GetEnumerator()));

            // Mock the context
            _mockContext.Setup(m => m.Bcards).Returns(mockCardSet.Object);

            // Act
            var result = await _controller.ExportCardToCSV();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Contains("Bcards.csv", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportCardsToXML_ReturnsXMLFile()
        {
            // Arrange
            var records = new List<Bcard>()
            {
                new Bcard { Id = 1, Name = "raneem", Gender = "Female", Birth = DateTime.Parse("1995-01-01"), Email = "raneem@example.com", Phone = "1234567890", Photo = "photo1.jpg", Address = "Address 1" },
                new Bcard { Id = 2, Name = "ahmad", Gender = "Male", Birth = DateTime.Parse("1990-01-01"), Email = "ahmad@example.com", Phone = "0987654321", Photo = "photo2.jpg", Address = "Address 2" }
            }.AsQueryable();

            // Mock the DbSet to support async operations
            var mockCardSet = new Mock<DbSet<Bcard>>();
            mockCardSet.As<IQueryable<Bcard>>()
                .Setup(m => m.Provider)
                .Returns(records.Provider);
            mockCardSet.As<IQueryable<Bcard>>()
                .Setup(m => m.Expression)
                .Returns(records.Expression);
            mockCardSet.As<IQueryable<Bcard>>()
                .Setup(m => m.ElementType)
                .Returns(records.ElementType);
            mockCardSet.As<IQueryable<Bcard>>()
                .Setup(m => m.GetEnumerator())
                .Returns(records.GetEnumerator());

            // Mock the async enumeration support
            mockCardSet.As<IAsyncEnumerable<Bcard>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Bcard>(records.GetEnumerator()));

            // Mock the context
            _mockContext.Setup(m => m.Bcards).Returns(mockCardSet.Object);

            // Act
            var result = await _controller.ExportCardsToXML();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/xml", fileResult.ContentType);
            Assert.Contains("Bcards.xml", fileResult.FileDownloadName);

            // Optionally, verify the XML content
            var fileBytes = fileResult.FileContents;
            var xmlContent = System.Text.Encoding.UTF8.GetString(fileBytes);

            // Check for expected XML structure
            Assert.Contains("<DocumentElement>", xmlContent);
            Assert.Contains("<Bcard>", xmlContent);
            Assert.Contains("<Name>raneem</Name>", xmlContent);
            Assert.Contains("<Email>raneem@example.com</Email>", xmlContent);
            Assert.Contains("<Name>ahmad</Name>", xmlContent);
            Assert.Contains("<Email>ahmad@example.com</Email>", xmlContent);
        }


    }
}