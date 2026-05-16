using System;
using System.Collections.Generic;

namespace MathApi_Client.Models;

public partial class MathCalculation
{
    // 1. Make the constructor private so objects can't be created with 'new' elsewhere
    private MathCalculation() { }

    public int CalculationId { get; set; }

    public decimal? FirstNumber { get; set; }

    public decimal? SecondNumber { get; set; }

    public int? Operation { get; set; }

    public decimal? Result { get; set; }

    public string? FirebaseUuid { get; set; }

    // 2. The Static Factory Method
    public static MathCalculation Create(decimal? firstNumber, decimal? secondNumber, int? operation, decimal? result, string? firebaseUuid)
    {
        // Validation logic: 4 is the ID for division in your GetOperations list
        if (operation == 4 && secondNumber == 0)
        {
            throw new ArgumentException("Cannot divide by zero.");
        }

        return new MathCalculation
        {
            FirstNumber = firstNumber,
            SecondNumber = secondNumber,
            Operation = operation,
            Result = result,
            FirebaseUuid = firebaseUuid
        };
    }
}