using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryApi.Entities;
using LibraryApi.Repositories;
using LibraryApi.Services;
using Moq;
using NUnit.Framework;

namespace LibraryApi.Tests
{
    [TestFixture]
    public class BookServiceTests
    {
        private Mock<IBookRepository> _mockRepo = null!;
        private IBookService _service = null!;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IBookRepository>();
            _service = new BookService(_mockRepo.Object);
        }

        // --- ГРУПА 1: CRUD операції Book ---

        [Test]
        public async Task GetAllAsync_ReturnsAllBooks()
        {
            // ARRANGE
            var books = new List<Book> { new Book { Title = "Book1" } };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

            // ACT
            var result = await _service.GetAllAsync();
            var listResult = result.ToList();

            // ASSERT
            Assert.That(listResult, Has.Count.EqualTo(1), "Перевіряємо, що повертається правильна кількість книг");
            Assert.That(listResult[0].Title, Is.EqualTo("Book1"), "Перевіряємо коректність назви книги");
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsBook()
        {
            // ARRANGE
            var book = new Book { Id = Guid.NewGuid(), Title = "Book1" };
            _mockRepo.Setup(r => r.GetByIdAsync(book.Id)).ReturnsAsync(book);

            // ACT
            var result = await _service.GetByIdAsync(book.Id);

            // ASSERT
            Assert.That(result, Is.Not.Null, "Книга існує і не повинна бути null");
            Assert.That(result!.Title, Is.EqualTo("Book1"), "Перевіряємо коректність назви книги");
        }

        [Test]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // ARRANGE
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Book?)null);

            // ACT
            var result = await _service.GetByIdAsync(Guid.NewGuid());

            // ASSERT
            Assert.That(result, Is.Null, "Перевіряємо, що неіснуючий ID повертає null");
        }

        [Test]
        public async Task CreateAsync_AssignsNewIdAndCallsAddAsync()
        {
            // ARRANGE
            var book = new Book { Title = "New Book" };

            // ACT
            await _service.CreateAsync(book);

            // ASSERT
            _mockRepo.Verify(r => r.AddAsync(It.Is<Book>(b => b.Title == "New Book" && b.Id != Guid.Empty)), Times.Once,
                "Перевіряємо, що метод AddAsync викликається один раз і ID генерується");
        }

        [Test]
        public async Task UpdateAsync_ValidBook_CallsUpdateOnce()
        {
            // ARRANGE
            var book = new Book { Id = Guid.NewGuid(), Title = "Updated" };
            _mockRepo.Setup(r => r.UpdateAsync(book)).ReturnsAsync(true);

            // ACT
            var result = await _service.UpdateAsync(book);

            // ASSERT
            Assert.That(result, Is.True, "Метод UpdateAsync повинен повертати true");
            _mockRepo.Verify(r => r.UpdateAsync(book), Times.Once, "Перевіряємо, що метод викликається лише один раз");
        }

        [Test]
        public async Task DeleteAsync_ExistingId_CallsDeleteOnce()
        {
            // ARRANGE
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

            // ACT
            var result = await _service.DeleteAsync(id);

            // ASSERT
            Assert.That(result, Is.True, "Видалення існуючої книги має повернути true");
            _mockRepo.Verify(r => r.DeleteAsync(id), Times.Once, "Перевіряємо, що DeleteAsync викликається один раз");
        }

        [Test]
        public async Task DeleteAsync_NonExistingId_ReturnsFalse()
        {
            // ARRANGE
            _mockRepo.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            // ACT
            var result = await _service.DeleteAsync(Guid.NewGuid());

            // ASSERT
            Assert.That(result, Is.False, "Видалення неіснуючої книги має повернути false");
        }
    }
}
