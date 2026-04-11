import type { ComponentType } from 'react';
import type { BeachDto, FavoriteDto } from '../types';

declare const BeachCard: ComponentType<{
  beach: BeachDto | FavoriteDto;
}>;

export default BeachCard;
