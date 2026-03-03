import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../../services/group_service.dart';
import '../../services/location_service.dart';
import '../../models/group_model.dart';
import '../groups/group_details_screen.dart';

  void _showEmergencyCoordinates(BuildContext context, LocationService locationService) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Row(
            children: [
              Icon(Icons.emergency, color: Colors.red[700]),
              const SizedBox(width: 8),
              const Text('Emergency Coordinates'),
            ],
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Precise GPS coordinates for emergency services:',
                style: TextStyle(fontWeight: FontWeight.w500),
              ),
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.red[50],
                  border: Border.all(color: Colors.red[200]!),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'GPS Coordinates:',
                      style: TextStyle(
                        fontWeight: FontWeight.w600,
                        color: Colors.red[800],
                      ),
                    ),
                    const SizedBox(height: 4),
                    SelectableText(
                      locationService.getPreciseCoordinates(),
                      style: const TextStyle(
                        fontFamily: 'monospace',
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 8),
                    if (locationService.currentPosition != null) ...[
                      Text(
                        'Accuracy: ${locationService.currentPosition!.accuracy.toStringAsFixed(1)}m',
                        style: TextStyle(
                          color: Colors.red[700],
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ],
                ),
              ),
              const SizedBox(height: 12),
              const Text(
                '⚠️ These coordinates are for emergency use only. Share with emergency services when needed.',
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.black54,
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () {
                // Copy coordinates to clipboard
                Clipboard.setData(ClipboardData(
                  text: locationService.getPreciseCoordinates(),
                ));
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Coordinates copied to clipboard'),
                    backgroundColor: Colors.green,
                  ),
                );
              },
              child: const Text('Copy'),
            ),
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Close'),
            ),
          ],
        );
      },
    );
  } 

  void _triggerSOS(BuildContext context, LocationService locationService) {
    if (locationService.currentPosition == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Location not available. Please wait for GPS lock.'),
          backgroundColor: Colors.orange,
        ),
      );
      return;
    }

    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Row(
            children: [
              Icon(Icons.warning, color: Colors.red[700]),
              const SizedBox(width: 8),
              const Text('Emergency SOS'),
            ],
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'This will send an emergency alert with your precise location to your safety group and emergency contacts.',
                style: TextStyle(fontWeight: FontWeight.w500),
              ),
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.red[50],
                  border: Border.all(color: Colors.red[200]!),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Your Location:',
                      style: TextStyle(
                        fontWeight: FontWeight.w600,
                        color: Colors.red[800],
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      locationService.getDisplayLocation(),
                      style: const TextStyle(fontSize: 14),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'GPS: ${locationService.getPreciseCoordinates()}',
                      style: const TextStyle(
                        fontFamily: 'monospace',
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () {
                // TODO: Implement actual SOS API call with precise coordinates
                Navigator.of(context).pop();
                _sendSOSAlert(locationService);
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.red[600],
                foregroundColor: Colors.white,
              ),
              child: const Text('SEND SOS'),
            ),
          ],
        );
      },
    );
  }

  void _sendSOSAlert(LocationService locationService) {
    // TODO: Implement actual SOS API call
    // This should send precise coordinates to emergency services
    debugPrint('🚨 SOS TRIGGERED!');
    debugPrint('📍 Emergency Location: ${locationService.getPreciseCoordinates()}');
    debugPrint('🏠 Address: ${locationService.getDisplayLocation()}');
    debugPrint('🎯 Accuracy: ${locationService.currentPosition?.accuracy}m');
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: const Text('🚨 Emergency SOS sent with precise location!'),
        backgroundColor: Colors.red[600],
        duration: const Duration(seconds: 3),
      ),
    );
  } 