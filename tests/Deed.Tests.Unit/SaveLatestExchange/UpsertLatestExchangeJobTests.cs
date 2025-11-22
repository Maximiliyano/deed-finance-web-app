using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Exchanges.Service;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Deed.Infrastructure.BackgroundJobs.SaveLatestExchange;
using NSubstitute;
using Quartz;

namespace Deed.Tests.Unit.SaveLatestExchange;

public sealed class UpsertLatestExchangeJobTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IExchangeRepository _repository = Substitute.For<IExchangeRepository>();
    private readonly IExchangeHttpService _service = Substitute.For<IExchangeHttpService>();
    private readonly IJobExecutionContext _context = Substitute.For<IJobExecutionContext>();

    private readonly UpsertLatestExchangeJob _job;

    public UpsertLatestExchangeJobTests()
    {
        _job = new UpsertLatestExchangeJob(_unitOfWork, _repository, _service);
    }

    [Fact]
    public async Task Execute_ShouldNotModifyDatabase_WhenApiFails()
    {
        // Arrange
        _service.GetCurrenciesAsync().Returns(Result.Failure<IEnumerable<Exchange>>(DomainErrors.Exchange.HttpExecution));

        // Act
        await _job.Execute(_context);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _repository.DidNotReceive().CreateRange(Arg.Any<IEnumerable<Exchange>>());
        _repository.DidNotReceive().UpdateRange(Arg.Any<IEnumerable<Exchange>>());
    }

    [Fact]
    public async Task Execute_ShouldInsertAll_WhenNoExistingRecords()
    {
        // Arrange
        var newRates = new[]
        {
            new Exchange(1) { NationalCurrencyCode = "USD", TargetCurrencyCode = "UAH", Buy = 40, Sale = 41 },
            new Exchange(2) { NationalCurrencyCode = "EUR", TargetCurrencyCode = "UAH", Buy = 43, Sale = 44 }
        };

        _service.GetCurrenciesAsync().Returns(Result.Success<IEnumerable<Exchange>>(newRates));
        _repository.GetAllAsync().Returns(new List<Exchange>());

        // Act
        await _job.Execute(_context);

        // Assert
        _repository.Received(1).CreateRange(Arg.Is<IEnumerable<Exchange>>(e => e.Count() == 2));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ShouldNotUpdate_WhenValuesAreUnchanged()
    {
        var existing = new Exchange(1) { NationalCurrencyCode = "USD", TargetCurrencyCode = "UAH", Buy = 40, Sale = 41 };

        _service.GetCurrenciesAsync().Returns(Result.Success<IEnumerable<Exchange>>(new[] { existing }));
        _repository.GetAllAsync().Returns([existing]);

        // Act
        await _job.Execute(_context);

        // Assert
        _repository.DidNotReceive().CreateRange(Arg.Any<IEnumerable<Exchange>>());
        _repository.DidNotReceive().UpdateRange(Arg.Any<IEnumerable<Exchange>>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(default);
    }
    
    [Fact]
    public async Task Execute_ShouldUpdate_WhenValuesChanged()
    {
        var existing = new Exchange(1) { NationalCurrencyCode = "USD", TargetCurrencyCode = "UAH", Buy = 40, Sale = 41 };
        var latest = new Exchange(1) { NationalCurrencyCode = "USD", TargetCurrencyCode = "UAH", Buy = 42, Sale = 43 };

        _service.GetCurrenciesAsync().Returns(Result.Success<IEnumerable<Exchange>>(new[] { latest }));
        _repository.GetAllAsync().Returns(new[] { existing });

        // Act
        await _job.Execute(_context);

        // Assert
        _repository.Received(1).UpdateRange(Arg.Is<IEnumerable<Exchange>>(e =>
            e.Single().Buy == 42 && e.Single().Sale == 43
        ));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ShouldInsertAndUpdate_WhenMixed()
    {
        var existing = new Exchange(1) { NationalCurrencyCode = "USD", TargetCurrencyCode = "UAH", Buy = 40, Sale = 41 };
        var updated = new Exchange(1) { NationalCurrencyCode = "USD", TargetCurrencyCode = "UAH", Buy = 42, Sale = 43 };
        var added = new Exchange(1) { NationalCurrencyCode = "EUR", TargetCurrencyCode = "UAH", Buy = 43, Sale = 44 };

        _service.GetCurrenciesAsync().Returns(Result.Success<IEnumerable<Exchange>>(new[] { updated, added }));
        _repository.GetAllAsync().Returns(new[] { existing });

        // Act
        await _job.Execute(_context);

        // Assert
        _repository.Received(1).UpdateRange(Arg.Any<IEnumerable<Exchange>>());
        _repository.Received(1).CreateRange(Arg.Is<IEnumerable<Exchange>>(e => e.Single().NationalCurrencyCode == "EUR"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Execute_ShouldDoNothing_WhenRemoteListIsEmpty()
    {
        _service.GetCurrenciesAsync().Returns(Result.Success<IEnumerable<Exchange>>(Array.Empty<Exchange>()));
        _repository.GetAllAsync().Returns(new List<Exchange>());

        // Act
        await _job.Execute(_context);

        // Assert
        _repository.DidNotReceive().CreateRange(Arg.Any<IEnumerable<Exchange>>());
        _repository.DidNotReceive().UpdateRange(Arg.Any<IEnumerable<Exchange>>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
