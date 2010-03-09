// testing code pulled from Fohjin - http://github.com/MarkNijhof/Fohjin
using System;

using NUnit.Framework;

namespace UnitTests
{
    [Specification]
    public abstract class BaseTestFixture
    {
        protected Exception _caughtException;
        protected virtual void Given() { }
        protected abstract void When();
        protected virtual void Finally() { }

        [Given]
        public void Setup()
        {
            Given();

            try
            {
                When();
            }
            catch (Exception exception)
            {
                _caughtException = exception;
            }
            finally
            {
                Finally();
            }
        }
    }

    [Specification]
    public abstract class BaseTestFixture<TSubjectUnderTest> where TSubjectUnderTest : class
    {
        protected TSubjectUnderTest _subjectUnderTest;
        protected Exception _caughtException;

        protected virtual void SetupDependencies() { }
        protected virtual void Given() { }
        protected abstract void When();
        protected virtual void Finally() { }

        [Given]
        public void Setup()
        {
            SetupDependencies();
            Given();

            try
            {
                When();
            }
            catch (Exception exception)
            {
                _caughtException = exception;
            }
            finally
            {
                Finally();
            }
        }
    }

    public class GivenAttribute : SetUpAttribute { }

    public class ThenAttribute : TestAttribute { }

    public class SpecificationAttribute : TestFixtureAttribute { }
}