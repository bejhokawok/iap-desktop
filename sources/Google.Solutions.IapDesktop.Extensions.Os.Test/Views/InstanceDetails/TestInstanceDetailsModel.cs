﻿//
// Copyright 2020 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Apis.Auth.OAuth2;
using Google.Solutions.Common.Locator;
using Google.Solutions.Common.Test;
using Google.Solutions.Common.Test.Integration;
using Google.Solutions.IapDesktop.Application.Services.Adapters;
using Google.Solutions.IapDesktop.Extensions.Os.Services.Inventory;
using Google.Solutions.IapDesktop.Extensions.Os.Views.InstanceDetails;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.IapDesktop.Extensions.Os.Test.Views.InstanceDetails
{
    [TestFixture]
    [Category("IntegrationTest")]
    public class TestInstanceDetailsModel : FixtureBase
    {
        [Test]
        public async Task WhenLoadAsyncCompletes_ThenPropertiesArePopulated(
            [WindowsInstance] ResourceTask<InstanceLocator> testInstance,
            [Credential(Role = PredefinedRole.ComputeViewer)] ResourceTask<ICredential> credential)
        {
            var locator = await testInstance;

            var gceAdapter = new ComputeEngineAdapter(await credential);
            var model = await InstanceDetailsModel.LoadAsync(
                await testInstance,
                gceAdapter,
                new InventoryService(gceAdapter),
                CancellationToken.None);

            Assert.AreEqual(locator.Name, model.InstanceName);
            Assert.IsNull(model.Hostname);
            Assert.AreEqual("RUNNING", model.Status);
            Assert.IsNotNull(model.InternalIp);
            Assert.IsNotNull(model.ExternalIp);
            Assert.IsNotNull(model.Licenses);
            Assert.IsFalse(model.IsSoleTenant);
        }
    }
}
