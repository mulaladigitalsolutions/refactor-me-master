﻿using System;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Application.RiskStrategies
{
    public class LateThenLateStrategy : IRiskStrategy
    {
        public bool IsMatch(Book book, DateTime dateReturned)
        {
            return book.LastReturnedDate > book.LastDueDate
                && book.LastDueDate.HasValue
                && book.LastReturnedDate.HasValue
                && book.CurrentDueDate < dateReturned;
        }

        public RiskAssessmentResult Evaluate(Book book, DateTime dateReturned)
        {
            return new RiskAssessmentResult
            {
                Level = RiskLevel.High,
                Reason = "returned late last time and late this time"
            };
        }
    }
}