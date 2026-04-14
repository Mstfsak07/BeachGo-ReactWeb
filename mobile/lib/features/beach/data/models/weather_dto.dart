import 'package:beachgo/features/beach/domain/entities/weather.dart';

class WeatherDto {
  const WeatherDto({
    required this.temperature,
    required this.description,
    required this.windSpeed,
    required this.seaTemperature,
    required this.waveHeight,
  });

  final double? temperature;
  final String description;
  final double? windSpeed;
  final double? seaTemperature;
  final double? waveHeight;

  factory WeatherDto.fromJson(Map<String, dynamic> json) {
    final weatherJson = json['weather'];
    final seaJson = json['sea'];
    final weatherMap =
        weatherJson is Map<String, dynamic> ? weatherJson : const <String, dynamic>{};
    final seaMap =
        seaJson is Map<String, dynamic> ? seaJson : const <String, dynamic>{};

    return WeatherDto(
      temperature: _asDouble(weatherMap['temperature'] ?? weatherMap['temp']),
      description:
          (weatherMap['description'] ?? weatherMap['condition'])?.toString() ?? '',
      windSpeed: _asDouble(weatherMap['windSpeed']),
      seaTemperature:
          _asDouble(seaMap['seaTemperature'] ?? seaMap['temperature']),
      waveHeight: _asDouble(seaMap['waveHeight']),
    );
  }

  static double? _asDouble(Object? value) {
    if (value is double) return value;
    if (value is num) return value.toDouble();
    return double.tryParse(value?.toString() ?? '');
  }
}

extension WeatherDtoMapper on WeatherDto {
  Weather toDomain() {
    return Weather(
      temperature: temperature,
      description: description,
      windSpeed: windSpeed,
      seaTemperature: seaTemperature,
      waveHeight: waveHeight,
    );
  }
}
