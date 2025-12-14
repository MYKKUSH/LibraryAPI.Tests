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
    public class AuthorServiceTests
    {
        private Mock<IAuthorRepository> _mockRepo = null!;
        private IAuthorService _service = null!;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IAuthorRepository>();
            _service = new AuthorService(_mockRepo.Object);
        }

        // --- ГРУПА 1: CRUD операції Author ---

        [Test]
        public async Task GetAllAsync_ReturnsAllAuthors()
        {
            // ARRANGE
            var authors = new List<Author> { new Author { Name = "Author1" } };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(authors);

            // ACT
            var result = await _service.GetAllAsync();
            var listResult = result.ToList();

            // ASSERT
            Assert.That(listResult, Has.Count.EqualTo(1), "Перевіряємо, що повертається правильна кількість авторів");
            Assert.That(listResult[0].Name, Is.EqualTo("Author1"), "Перевіряємо, що ім'я автора збігається");
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsAuthor()
        {
            // ARRANGE
            var author = new Author { Id = Guid.NewGuid(), Name = "John" };
            _mockRepo.Setup(r => r.GetByIdAsync(author.Id)).ReturnsAsync(author);

            // ACT
            var result = await _service.GetByIdAsync(author.Id);

            // ASSERT
            Assert.That(result, Is.Not.Null, "Автор існує і не повинен бути null");
            Assert.That(result!.Name, Is.EqualTo("John"), "Перевіряємо коректність імені");
        }

        [Test]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // ARRANGE
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Author?)null);

            // ACT
            var result = await _service.GetByIdAsync(Guid.NewGuid());

            // ASSERT
            Assert.That(result, Is.Null, "Перевіряємо, що неіснуючий ID повертає null");
        }

        [Test]
        public async Task CreateAsync_AssignsNewIdAndCallsAddAsync()
        {
            // ARRANGE
            var author = new Author { Name = "New Author" };

            // ACT
            await _service.CreateAsync(author);

            // ASSERT
            _mockRepo.Verify(r => r.AddAsync(It.Is<Author>(a => a.Name == "New Author" && a.Id != Guid.Empty)), Times.Once,
                "Перевіряємо, що метод AddAsync викликається один раз і ID генерується");
        }

        [Test]
        public async Task UpdateAsync_ValidAuthor_CallsUpdateOnce()
        {
            // ARRANGE
            var author = new Author { Id = Guid.NewGuid(), Name = "Updated" };
            _mockRepo.Setup(r => r.UpdateAsync(author)).ReturnsAsync(true);

            // ACT
            var result = await _service.UpdateAsync(author);

            // ASSERT
            Assert.That(result, Is.True, "Метод UpdateAsync повинен повертати true");
            _mockRepo.Verify(r => r.UpdateAsync(author), Times.Once, "Перевіряємо, що метод викликається лише один раз");
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
            Assert.That(result, Is.True, "Видалення існуючого автора має повернути true");
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
            Assert.That(result, Is.False, "Видалення неіснуючого автора має повернути false");
        }
    }
}
