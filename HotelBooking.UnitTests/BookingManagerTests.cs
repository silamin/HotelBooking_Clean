using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Collections.Generic;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public void FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Action act = () => bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = bookingRepository.GetAll().Where(
                b => b.RoomId == roomId
                && b.StartDate <= date
                && b.EndDate >= date
                && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }


        [Fact]
        public void CreateBooking_ValidBooking_ReturnsTrue()
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(12),
                EndDate = DateTime.Today.AddDays(15)
            };

            // Act
            bool result = bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CreateBooking_RoomUnavailable_ReturnsFalse()
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(12),
                EndDate = DateTime.Today.AddDays(15)
            };

            // Book all rooms for the test period
            foreach (var room in bookingRepository.GetAll().Select(b => b.RoomId).Distinct())
            {
                bookingRepository.Add(new Booking
                {
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    RoomId = room,
                    IsActive = true
                });
            }

            // Act
            bool result = bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFullyOccupiedDates_ValidDates_ReturnsOccupiedDates()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(12);
            DateTime endDate = DateTime.Today.AddDays(15);

            // Book all rooms for the test period
            foreach (var room in bookingRepository.GetAll().Select(b => b.RoomId).Distinct())
            {
                bookingRepository.Add(new Booking
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    RoomId = room,
                    IsActive = true
                });
            }

            // Act
            List<DateTime> fullyOccupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Equal(4, fullyOccupiedDates.Count); // 4 days fully occupied
        }
    }
}
