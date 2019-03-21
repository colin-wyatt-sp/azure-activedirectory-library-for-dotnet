﻿//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Test.Common.Core.Mocks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;

namespace Test.ADAL.NET.Integration
{
    [TestClass]
    public class DeviceCodeFlowTests
    {
        private AuthenticationContext context;

        [TestInitialize]
        public void Initialize()
        {
            TokenCache.DefaultShared.Clear();
            InstanceDiscovery.InstanceCache.Clear();
        }

        [TestCleanup()]
        public void Cleanup()
        {
            if (context != null && context.TokenCache != null)
            {
                context.TokenCache.Clear();
            }
        }
        internal void SetupMocks(MockHttpManager httpManager)
        {
            httpManager.AddInstanceDiscoveryMockHandler();
        }

        [TestMethod]
        public async Task PositiveTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                DeviceCodeResult dcr = new DeviceCodeResult()
                {
                    ClientId = AdalTestConstants.DefaultClientId,
                    Resource = AdalTestConstants.DefaultResource,

                    DeviceCode = "device-code",
                    ExpiresOn = (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)),
                    Interval = 5,
                    Message = "get token here",
                    UserCode = "user-code",
                    VerificationUrl = "https://login.microsoftonline.com/home.oauth2/token"
                };

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.microsoftonline.com/home/oauth2/token",
                    ResponseMessage = MockHelpers.CreateFailureResponseMessage("{\"error\":\"authorization_pending\"," +
                                                                               "\"error_description\":\"AADSTS70016: Pending end-user authorization." +
                                                                               "\\r\\nTrace ID: f6c2c73f-a21d-474e-a71f-d8b121a58205\\r\\nCorrelation ID: " +
                                                                               "36fe3e82-442f-4418-b9f4-9f4b9295831d\\r\\nTimestamp: 2015-09-24 19:51:51Z\"," +
                                                                               "\"error_codes\":[70016],\"timestamp\":\"2015-09-24 19:51:51Z\",\"trace_id\":" +
                                                                               "\"f6c2c73f-a21d-474e-a71f-d8b121a58205\",\"correlation_id\":" +
                                                                               "\"36fe3e82-442f-4418-b9f4-9f4b9295831d\"}")
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.microsoftonline.com/home/oauth2/token",
                    ResponseMessage =
                        MockHelpers.CreateSuccessTokenResponseMessage(AdalTestConstants.DefaultUniqueId,
                            AdalTestConstants.DefaultDisplayableId, AdalTestConstants.DefaultResource)
                });

                TokenCache cache = new TokenCache();
                context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                    AuthorityValidationType.NotProvided,
                    cache);
                AuthenticationResult result = await context.AcquireTokenByDeviceCodeAsync(dcr).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual("some-access-token", result.AccessToken);
            }
        }

        [TestMethod]
        public async Task FullCoveragePositiveTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/devicecode",
                    ResponseMessage = MockHelpers.CreateSuccessDeviceCodeResponseMessage()
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/token",
                    ResponseMessage = MockHelpers.CreateFailureResponseMessage("{\"error\":\"authorization_pending\"," +
                                                                   "\"error_description\":\"AADSTS70016: Pending end-user authorization." +
                                                                   "\\r\\nTrace ID: f6c2c73f-a21d-474e-a71f-d8b121a58205\\r\\nCorrelation ID: " +
                                                                   "36fe3e82-442f-4418-b9f4-9f4b9295831d\\r\\nTimestamp: 2015-09-24 19:51:51Z\"," +
                                                                   "\"error_codes\":[70016],\"timestamp\":\"2015-09-24 19:51:51Z\",\"trace_id\":" +
                                                                   "\"f6c2c73f-a21d-474e-a71f-d8b121a58205\",\"correlation_id\":" +
                                                                   "\"36fe3e82-442f-4418-b9f4-9f4b9295831d\"}")
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/token",
                    ResponseMessage =
                        MockHelpers.CreateSuccessTokenResponseMessage(AdalTestConstants.DefaultUniqueId,
                            AdalTestConstants.DefaultDisplayableId, AdalTestConstants.DefaultResource)
                });

                TokenCache cache = new TokenCache();
                context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                    AuthorityValidationType.NotProvided,
                    cache);

                DeviceCodeResult dcr = await context.AcquireDeviceCodeAsync("some-resource", "some-client").ConfigureAwait(false);

                Assert.IsNotNull(dcr);
                Assert.AreEqual("some-device-code", dcr.DeviceCode);
                Assert.AreEqual("some-user-code", dcr.UserCode);
                Assert.AreEqual("some-URL", dcr.VerificationUrl);
                Assert.AreEqual(5, dcr.Interval);
                Assert.AreEqual("some-message", dcr.Message);
                Assert.AreEqual("some-client", dcr.ClientId);

                AuthenticationResult result = await context.AcquireTokenByDeviceCodeAsync(dcr).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual("some-access-token", result.AccessToken);
                // There should be one cached entry.
                Assert.AreEqual(1, context.TokenCache.Count);
            }
        }

        [TestMethod]
        public void NegativeDeviceCodeTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/devicecode",
                    ResponseMessage = MockHelpers.CreateDeviceCodeErrorResponse()
                });

                TokenCache cache = new TokenCache();
                context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                    AuthorityValidationType.NotProvided,
                    cache);
                DeviceCodeResult dcr;
                AdalServiceException ex = AssertException.TaskThrows<AdalServiceException>(async () => dcr = await context.AcquireDeviceCodeAsync("some-resource", "some-client").ConfigureAwait(false));
                Assert.IsTrue(ex.Message.Contains("some error message."));
            }
        }

        [TestMethod]
        public async Task NegativeDeviceCodeTimeoutTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/devicecode",
                    ResponseMessage = MockHelpers.CreateSuccessDeviceCodeResponseMessage(
                    // do not lower this to 1-2s as test execution may be slow and the flow 
                    // will never call the server
                    expirationTimeInSeconds: 30,
                    retryInternvalInSeconds: 2)
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/token",
                    ResponseMessage = MockHelpers.CreateFailureResponseMessage("{\"error\":\"authorization_pending\"," +
                                                                   "\"error_description\":\"AADSTS70016: Pending end-user authorization." +
                                                                   "\\r\\nTrace ID: f6c2c73f-a21d-474e-a71f-d8b121a58205\\r\\nCorrelation ID: " +
                                                                   "36fe3e82-442f-4418-b9f4-9f4b9295831d\\r\\nTimestamp: 2015-09-24 19:51:51Z\"," +
                                                                   "\"error_codes\":[70016],\"timestamp\":\"2015-09-24 19:51:51Z\",\"trace_id\":" +
                                                                   "\"f6c2c73f-a21d-474e-a71f-d8b121a58205\",\"correlation_id\":" +
                                                                   "\"36fe3e82-442f-4418-b9f4-9f4b9295831d\"}")
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/token",
                    ResponseMessage = MockHelpers.CreateDeviceCodeExpirationErrorResponse()
                });

                TokenCache cache = new TokenCache();

                context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                     AuthorityValidationType.NotProvided,
                    cache);

                DeviceCodeResult dcr = await context.AcquireDeviceCodeAsync("some resource", "some authority").ConfigureAwait(false);

                Assert.IsNotNull(dcr);
                AuthenticationResult result;
                AdalServiceException ex = AssertException.TaskThrows<AdalServiceException>(async () => result = await context.AcquireTokenByDeviceCodeAsync(dcr).ConfigureAwait(false));
                Assert.AreEqual(AdalError.DeviceCodeAuthorizationCodeExpired, ex.ErrorCode);
            }
        }

        [TestMethod]
        public async Task NegativeDeviceCodeTimeoutTest_WithZeroRetriesAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityHomeTenant)
                {
                    Method = HttpMethod.Post,
                    Url = AdalTestConstants.DefaultAuthorityHomeTenant + "oauth2/devicecode",
                    ResponseMessage = MockHelpers.CreateSuccessDeviceCodeResponseMessage(
                    expirationTimeInSeconds: 0,
                    retryInternvalInSeconds: 1)
                });

                TokenCache cache = new TokenCache();
                context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                    AuthorityValidationType.NotProvided,
                    cache);
                DeviceCodeResult dcr = await context.AcquireDeviceCodeAsync("some resource", "some authority").ConfigureAwait(false);

                Assert.IsNotNull(dcr);
                AuthenticationResult result;
                AdalServiceException ex = AssertException.TaskThrows<AdalServiceException>(async () => result = await context.AcquireTokenByDeviceCodeAsync(dcr).ConfigureAwait(false));
                Assert.AreEqual(AdalError.DeviceCodeAuthorizationCodeExpired, ex.ErrorCode);
            }
        }
    }
}