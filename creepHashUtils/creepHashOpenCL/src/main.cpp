// ==========================================================================
// 
// MultiMinerOpenCL - OpenCL utility tool for MultiCryptoTool
// Copyright (C) 2018
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation,
// Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110 - 1301  USA
// 
// ==========================================================================

#include <CL/cl.h>
#include <CL/cl_ext.h>
#include <iostream>
#include <string>
#include <vector>

using namespace std;

int show_usage();
bool is_arg(const string& arg, const string& excepted);
bool is_arg(const string& arg, const string& excepted, size_t& param);
template <typename TSubject, typename TInfoFunc, typename TArg>
static bool get_info(const TSubject& subject, std::string& info, TInfoFunc info_func, TArg arg, int& ret);

int main(const int argn, const char** argv)
{
	const auto divider = ';';

	if (argn != 2)
		return show_usage();

	size_t arg_platform_index;
	size_t arg_platform_devices_index;

	const auto arg = string(argv[1]);

	if (arg == "--help" || arg == "-h" || arg == "-?" || arg == "--?")
		return show_usage();

	const auto arg_platforms = is_arg(arg, "--platforms");
	const auto arg_devices = is_arg(arg, "--devices");
	const auto arg_platform = is_arg(arg, "--platform=", arg_platform_index);
	const auto arg_platform_devices = is_arg(arg, "--platform-devices=", arg_platform_devices_index);

	vector<cl_platform_id> platform_ids;
	vector<cl_device_id> device_ids;
	cl_uint platforms;
	cl_uint devices;
	
	auto error = clGetPlatformIDs(0, nullptr, &platforms);

	if (error != CL_SUCCESS)
	{
		cerr <<  "Could not get platforms size: errno " << error << endl;
		return 1;
	}
	
	if (platforms == 0)
	{
		cerr << "No valid OpenCL platforms detected!" << endl;
		return 1;
	}

	platform_ids.resize(platforms);

	error = clGetPlatformIDs(platforms, platform_ids.data(), nullptr);

	if (error != CL_SUCCESS)
	{
		cerr << "Could not get platforms: errno " << error << endl;
		return 1;
	}

	if (arg_platforms || arg_platform)
	{
		size_t i = 0;
		size_t end = platforms;

		if (arg_platform)
		{
			i = arg_platform_index;
			end = arg_platform_index + 1;
		}

		for (; i < end; ++i)
		{
			const auto& platform_id = platform_ids[i];
		
			string name;
			string version;

			if (get_info(platform_id, name, clGetPlatformInfo, CL_PLATFORM_NAME, error) &&
				get_info(platform_id, version, clGetPlatformInfo, CL_PLATFORM_VERSION, error))
				cout << i << divider
					 << name << divider
					 << version << endl;
		}
		
		return 0;
	}

	if (arg_devices || arg_platform_devices)
	{
		size_t p = 0;
		size_t p_end = platforms;

		if (arg_platform_devices)
		{
			p = arg_platform_devices_index;
			p_end = arg_platform_devices_index + 1;
		}

		for (; p < p_end; ++p)
		{
			const auto platform_id = platform_ids[p];

			error = clGetDeviceIDs(platform_id, CL_DEVICE_TYPE_GPU, 0, nullptr, &devices);

			if (error == CL_DEVICE_NOT_FOUND)
				continue;

			if (error != CL_SUCCESS)
			{
				cerr << "Could not detect the number of valid OpenCL devices: errno " << error << endl;
				return 1;
			}
			
			device_ids.resize(devices);
			error = clGetDeviceIDs(platform_id, CL_DEVICE_TYPE_GPU, devices, device_ids.data(), nullptr);

			if (error != CL_SUCCESS)
			{
				cerr << "Could not get devices: errno " << error << endl;
				return 1;
			}
		
			for (size_t i = 0; i < device_ids.size(); ++i)
			{
				const auto& device_id = device_ids[i];
				cl_device_topology_amd topology;
				string name;
				int pci_bus, pci_slot;
				auto pci_bus_slot_scanned = false;

				const auto topology_status = clGetDeviceInfo(device_id, CL_DEVICE_TOPOLOGY_AMD, sizeof(cl_device_topology_amd), &topology, nullptr);

				if (topology_status == CL_SUCCESS)
				{
					if (topology.raw.type == CL_DEVICE_TOPOLOGY_TYPE_PCIE_AMD)
					{
						pci_bus = static_cast<int>(topology.pcie.bus);
						pci_slot = static_cast<int>(topology.pcie.device);
						pci_bus_slot_scanned = true;
					}
				}


				if (!pci_bus_slot_scanned)
				{
					const auto pci_bus_status = clGetDeviceInfo(device_id, 0x4008, sizeof(int), &pci_bus, nullptr);
					const auto pci_slot_status = clGetDeviceInfo(device_id, 0x4009, sizeof(int), &pci_slot, nullptr);

					if (pci_bus_status != CL_SUCCESS || pci_slot_status != CL_SUCCESS)
					{
						cout << "Could not get PCI bus/slot for device " << device_id << ": errno: "
							<< pci_bus_status << "/" << pci_slot_status << endl;
						continue;
					}
				}

				if (get_info(device_id, name, clGetDeviceInfo, CL_DEVICE_NAME, error))
					cout << i << divider
						 << p << divider
						 << name << divider
						 << pci_bus << divider
						 << pci_slot
						 << endl;
			}
		}

		return 0;
	}

	return 1;
}

int show_usage()
{
	cout << "MultiMinerOpenCL [--platforms] [--devices] [--platform-devices=n]" << endl;
	return 1;
}

bool is_arg(const string& arg, const string& excepted)
{
	size_t dummy;
	return is_arg(arg, excepted, dummy);
}

bool is_arg(const string& arg, const string& excepted, size_t& param)
{
	if (excepted.size() > arg.size())
		return false;
	
	for (size_t i = 0; i < excepted.size(); ++i)
		if (excepted[i] != arg[i])
			return false;

	if (arg.size() > excepted.size())
	{
		try
		{
			param = stoul(arg.substr(excepted.size()));
		}
		catch (...)
		{
			return false;
		}
	}

	return true;
}

template <typename TSubject, typename TInfoFunc, typename TArg>
static bool get_info(const TSubject& subject, std::string& info, TInfoFunc info_func, TArg arg, int& ret)
{
	info = std::string(255, '\0');
	size_t size = 0;

	ret = info_func(subject, arg, info.size(), &info[0], &size);

	if (ret != CL_SUCCESS)
		return false;

	if (size > 0)
		--size;

	info.resize(size);
	return true;
}
