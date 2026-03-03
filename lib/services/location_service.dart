import 'package:flutter/foundation.dart';
import 'package:geolocator/geolocator.dart';
import 'package:geocoding/geocoding.dart';

class LocationService extends ChangeNotifier {
  Position? _currentPosition;
  String? _currentAddress;
  String? _currentCoordinates; // For emergency use
  bool _isLoading = false;
  String? _error;
  bool _isTracking = false;

  // Getters
  Position? get currentPosition => _currentPosition;
  String? get currentAddress => _currentAddress;
  String? get currentCoordinates => _currentCoordinates; // Emergency coordinates
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get isTracking => _isTracking;

  // Get precise coordinates for emergency use
  String getPreciseCoordinates() {
    if (_currentPosition != null) {
      return '${_currentPosition!.latitude.toStringAsFixed(6)}, ${_currentPosition!.longitude.toStringAsFixed(6)}';
    }
    return 'Location unavailable';
  }

  // Get display address for UI (user-friendly)
  String getDisplayLocation() {
    return _currentAddress ?? 'Current Location';
  }

  Future<void> getCurrentLocation() async {
    debugPrint('🎯 Getting current location for Haven safety app...');
    _setLoading(true);
    _error = null;

    try {
      // Check permissions
      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) {
          throw Exception('Location permissions are denied');
        }
      }

      if (permission == LocationPermission.deniedForever) {
        throw Exception('Location permissions are permanently denied');
      }

      // Get current position with high accuracy for emergency use
      debugPrint('📍 Getting high-accuracy position for safety...');
      _currentPosition = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
        timeLimit: const Duration(seconds: 10),
      );

      debugPrint('✅ Position obtained: ${_currentPosition!.latitude}, ${_currentPosition!.longitude}');
      debugPrint('🎯 Accuracy: ${_currentPosition!.accuracy} meters');

      // Store precise coordinates for emergency use
      _currentCoordinates = '${_currentPosition!.latitude.toStringAsFixed(6)}, ${_currentPosition!.longitude.toStringAsFixed(6)}';
      debugPrint('🚨 Emergency coordinates: $_currentCoordinates');

      // Get user-friendly address for display
      if (_currentPosition != null) {
        try {
          debugPrint('🏠 Getting user-friendly address for display...');
          _currentAddress = await _getAddressFromCoordinates(_currentPosition!);
          debugPrint('✅ Display address: $_currentAddress');
        } catch (e) {
          debugPrint('💥 Address lookup failed, using generic display: $e');
          _currentAddress = 'Current Location';
        }
      }

      notifyListeners();
    } catch (e) {
      debugPrint('💥 Location error: $e');
      _error = e.toString();
      _currentAddress = 'Location unavailable';
      _currentCoordinates = 'Location unavailable';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> startLocationTracking() async {
    if (_isTracking) return;

    debugPrint('🔄 Starting location tracking for Haven safety...');
    _isTracking = true;
    notifyListeners();

    const LocationSettings locationSettings = LocationSettings(
      accuracy: LocationAccuracy.high, // High accuracy for safety
      distanceFilter: 5, // Update every 5 meters for safety
    );

    Geolocator.getPositionStream(locationSettings: locationSettings).listen(
      (Position position) async {
        debugPrint('📍 New position: ${position.latitude}, ${position.longitude}');
        _currentPosition = position;
        
        // Update precise coordinates for emergency use
        _currentCoordinates = '${position.latitude.toStringAsFixed(6)}, ${position.longitude.toStringAsFixed(6)}';
        debugPrint('🚨 Updated emergency coordinates: $_currentCoordinates');

        // Get user-friendly address for display
        try {
          _currentAddress = await _getAddressFromCoordinates(position);
        } catch (e) {
          debugPrint('Address lookup failed during tracking: $e');
          _currentAddress = 'Current Location';
        }

        notifyListeners();
      },
      onError: (e) {
        debugPrint('Location tracking error: $e');
        _error = e.toString();
        notifyListeners();
      },
    );
  }

  void stopLocationTracking() {
    debugPrint('⏹️ Stopping location tracking');
    _isTracking = false;
    notifyListeners();
  }

  Future<String?> _getAddressFromCoordinates(Position position) async {
    // Try geocoding up to 3 times for user-friendly address
    for (int attempt = 1; attempt <= 3; attempt++) {
      try {
        debugPrint('🌍 Getting display address (attempt $attempt): ${position.latitude}, ${position.longitude}');
        
        List<Placemark> placemarks = await placemarkFromCoordinates(
          position.latitude,
          position.longitude,
        );
        
        if (placemarks.isNotEmpty) {
          // Try each placemark to find the best address
          for (int i = 0; i < placemarks.length && i < 3; i++) {
            Placemark place = placemarks[i];
            
            // Try multiple combinations to build a readable address
            List<String> addressOptions = [];
            
            // Option 1: Full address with street number and name
            if (place.subThoroughfare != null && place.subThoroughfare!.trim().isNotEmpty &&
                place.thoroughfare != null && place.thoroughfare!.trim().isNotEmpty) {
              addressOptions.add('${place.subThoroughfare!.trim()} ${place.thoroughfare!.trim()}');
            } else if (place.thoroughfare != null && place.thoroughfare!.trim().isNotEmpty) {
              addressOptions.add(place.thoroughfare!.trim());
            } else if (place.street != null && place.street!.trim().isNotEmpty) {
              addressOptions.add(place.street!.trim());
            }
            
            // Option 2: Area/Neighborhood
            if (place.subLocality != null && place.subLocality!.trim().isNotEmpty) {
              if (!addressOptions.contains(place.subLocality!.trim())) {
                addressOptions.add(place.subLocality!.trim());
              }
            }
            
            // Option 3: City
            if (place.locality != null && place.locality!.trim().isNotEmpty) {
              if (!addressOptions.contains(place.locality!.trim())) {
                addressOptions.add(place.locality!.trim());
              }
            }
            
            // Option 4: State/Province
            if (place.administrativeArea != null && place.administrativeArea!.trim().isNotEmpty) {
              addressOptions.add(place.administrativeArea!.trim());
            }
            
            // Build final address (limit to 3 parts for readability)
            if (addressOptions.isNotEmpty) {
              final address = addressOptions.take(3).join(', ');
              debugPrint('🎉 Display address: $address');
              return address;
            }
            
            // Try place name as backup
            if (place.name != null && place.name!.trim().isNotEmpty) {
              debugPrint('🎉 Using place name: ${place.name}');
              return place.name!.trim();
            }
          }
        }
        
        // If this attempt failed, wait a bit before retrying
        if (attempt < 3) {
          await Future.delayed(const Duration(seconds: 1));
        }
        
      } catch (e) {
        debugPrint('💥 Geocoding attempt $attempt failed: $e');
        if (attempt < 3) {
          await Future.delayed(const Duration(seconds: 1));
        }
      }
    }
    
    // All attempts failed, return generic message for display
    debugPrint('⚠️ All geocoding attempts failed, using generic display message');
    return 'Current Location';
  }

  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }

  void clearError() {
    _error = null;
    notifyListeners();
  }
} 