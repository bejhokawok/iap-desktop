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

using Google.Solutions.Common.Locator;
using Google.Solutions.IapDesktop.Extensions.LogAnalysis.Events;
using Google.Solutions.IapDesktop.Extensions.LogAnalysis.History;
using Google.Solutions.IapDesktop.Extensions.LogAnalysis.Services.SchedulingReport;
using NUnit.Framework;
using System;
using System.Linq;

namespace Google.Solutions.IapDesktop.Extensions.LogAnalysis.Test.Services.SchedulingReport
{
    [TestFixture]
    public class TestReportInstancesTabViewModel : FixtureBase
    {
        private static readonly DateTime BaselineTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private ulong instanceIdSequence;

        private void AddExistingInstance(
            InstanceSetHistoryBuilder builder,
            int count, 
            Tenancies tenancy)
        {
            for (int i = 0; i < count; i++)
            {
                instanceIdSequence++;

                builder.AddExistingInstance(
                    instanceIdSequence,
                    new InstanceLocator("project", "zone", $"instance-{instanceIdSequence}"),
                    new ImageLocator("project", $"image-{instanceIdSequence}"),
                    InstanceState.Running, 
                    BaselineTime.AddDays(i), 
                    tenancy);
            }
        }

        private ReportViewModel CreateParentViewModel(
            int fleetInstanceCount,
            int soleTenantInstanceCount)
        {
            this.instanceIdSequence = 0;

            var builder = new InstanceSetHistoryBuilder(
                BaselineTime,
                BaselineTime.AddDays(7));

            AddExistingInstance(builder, fleetInstanceCount, Tenancies.Fleet);
            AddExistingInstance(builder, soleTenantInstanceCount, Tenancies.SoleTenant);

            return new ReportViewModel(new ReportArchive(builder.Build()));
        }

        [Test]
        public void WhenTenancyFilterSetInParent_ThenInstancesContainsMatchingInstances()
        {
            var parentViewModel = CreateParentViewModel(1, 2);
            parentViewModel.Repopulate();
            
            var viewModel = parentViewModel.InstanceReportPane;

            Assert.AreEqual(3, viewModel.Instances.Count());

            parentViewModel.IncludeFleetInstances = false;
            Assert.AreEqual(2, viewModel.Instances.Count());

            parentViewModel.IncludeSoleTenantInstances = false;
            Assert.AreEqual(0, viewModel.Instances.Count());

            parentViewModel.IncludeFleetInstances = true;
            Assert.AreEqual(1, viewModel.Instances.Count());
        }

        [Test]
        public void WhenOsFilterSetInParent_ThenInstancesContainsMatchingInstances()
        {
            var parentViewModel = CreateParentViewModel(3, 0);
            parentViewModel.Model.AddLicenseAnnotation(
                new ImageLocator("project", "image-1"),
                OperatingSystemTypes.Linux,
                LicenseTypes.Unknown);
            parentViewModel.Model.AddLicenseAnnotation(
                new ImageLocator("project", "image-2"),
                OperatingSystemTypes.Windows,
                LicenseTypes.Unknown);
            parentViewModel.Model.AddLicenseAnnotation(
                new ImageLocator("project", "image-3"),
                OperatingSystemTypes.Unknown,
                LicenseTypes.Unknown);

            parentViewModel.Repopulate();

            var viewModel = parentViewModel.InstanceReportPane;

            parentViewModel.IncludeWindowsInstances = false;
            parentViewModel.IncludeLinuxInstances = false;
            parentViewModel.IncludeUnknownOsInstances = false;
            Assert.AreEqual(0, viewModel.Instances.Count());

            parentViewModel.IncludeWindowsInstances = true;
            parentViewModel.IncludeLinuxInstances = false;
            parentViewModel.IncludeUnknownOsInstances = false;
            Assert.AreEqual(1, viewModel.Instances.Count());

            parentViewModel.IncludeWindowsInstances = true;
            parentViewModel.IncludeLinuxInstances = true;
            parentViewModel.IncludeUnknownOsInstances = false;
            Assert.AreEqual(2, viewModel.Instances.Count());

            parentViewModel.IncludeWindowsInstances = true;
            parentViewModel.IncludeLinuxInstances = true;
            parentViewModel.IncludeUnknownOsInstances = true;
            Assert.AreEqual(3, viewModel.Instances.Count());
        }

        [Test]
        public void WhenLicenseFilterSetInParent_ThenInstancesContainsMatchingInstances()
        {
            var parentViewModel = CreateParentViewModel(3, 0);
            parentViewModel.Model.AddLicenseAnnotation(
                new ImageLocator("project", "image-1"),
                OperatingSystemTypes.Windows,
                LicenseTypes.Spla);
            parentViewModel.Model.AddLicenseAnnotation(
                new ImageLocator("project", "image-2"),
                OperatingSystemTypes.Windows,
                LicenseTypes.Byol);
            parentViewModel.Model.AddLicenseAnnotation(
                new ImageLocator("project", "image-3"),
                OperatingSystemTypes.Windows,
                LicenseTypes.Unknown);

            parentViewModel.Repopulate();

            var viewModel = parentViewModel.InstanceReportPane;

            parentViewModel.IncludeSplaInstances = false;
            parentViewModel.IncludeByolInstances = false;
            parentViewModel.IncludeUnknownLicensedInstances = false;
            Assert.AreEqual(0, viewModel.Instances.Count());

            parentViewModel.IncludeSplaInstances = true;
            parentViewModel.IncludeByolInstances = false;
            parentViewModel.IncludeUnknownLicensedInstances = false;
            Assert.AreEqual(1, viewModel.Instances.Count());

            parentViewModel.IncludeSplaInstances = true;
            parentViewModel.IncludeByolInstances = true;
            parentViewModel.IncludeUnknownLicensedInstances = false;
            Assert.AreEqual(2, viewModel.Instances.Count());

            parentViewModel.IncludeSplaInstances = true;
            parentViewModel.IncludeByolInstances = true;
            parentViewModel.IncludeUnknownLicensedInstances = true;
            Assert.AreEqual(3, viewModel.Instances.Count());
        }

        [Test]
        public void WhenTenancyFilterSetInParent_ThenHistogramContainsMatchingInstances()
        {
            var parentViewModel = CreateParentViewModel(1, 0);
            parentViewModel.Repopulate();

            var viewModel = parentViewModel.InstanceReportPane;

            Assert.AreEqual(1, viewModel.Histogram.First().Value);

            parentViewModel.IncludeFleetInstances = false;

            Assert.IsFalse(viewModel.Histogram.Any());
        }

        [Test]
        public void WhenDateRangeSelected_ThenInstancesContainsMatchingInstances()
        {
            var parentViewModel = CreateParentViewModel(3, 0);
            parentViewModel.Repopulate();

            var viewModel = parentViewModel.InstanceReportPane;
            Assert.AreEqual(3, viewModel.Instances.Count());

            viewModel.Selection = new DateSelection()
            {
                StartDate = BaselineTime,
                EndDate = BaselineTime.AddDays(1)
            };

            Assert.AreEqual(2, viewModel.Instances.Count());
        }

        [Test]
        public void WhenDateRangeSelected_ThenHistogramIsUnaffected()
        {
            var parentViewModel = CreateParentViewModel(3, 0);
            parentViewModel.Repopulate();

            var viewModel = parentViewModel.InstanceReportPane;

            var histogram = viewModel.Histogram;
            Assert.AreEqual(BaselineTime, histogram.First().Timestamp);
            Assert.AreEqual(BaselineTime.AddDays(2), histogram.Last().Timestamp);

            viewModel.Selection = new DateSelection()
            {
                StartDate = BaselineTime,
                EndDate = BaselineTime
            };

            Assert.AreEqual(BaselineTime, histogram.First().Timestamp);
            Assert.AreEqual(BaselineTime.AddDays(2), histogram.Last().Timestamp);
        }
    }
}
