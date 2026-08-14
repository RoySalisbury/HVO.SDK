Native assets included in this package:

- Linux ARM (`linux-arm`): Raspberry Pi 2 / ARMv7
- Linux ARM64 (`linux-arm64`): ARMv8
- macOS x64 (`osx-x64`): Intel 32/64-bit universal binary

Other platforms require a compatible ZWO ASI SDK native library installed separately.

$ sudo install asi.rules /lib/udev/rules.d
or
$ sudo install asi.rules /etc/udev/rules.d
and reconnect camera, then the camera can be opened without root
and run 'cat /sys/module/usbcore/parameters/usbfs_memory_mb' to make sure the result is 200

The version of libusb compiled with is 1.0.19
