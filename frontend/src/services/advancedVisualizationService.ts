/**
 * Advanced Visualization Service
 * 
 * This service provides enhanced visualization capabilities including
 * AI-powered chart recommendations, advanced configuration generation,
 * and intelligent data analysis for optimal chart selection.
 * 
 * Currently re-exports the base VisualizationService which already
 * contains all the advanced functionality.
 */

import visualizationService from './visualizationService';

// Re-export the visualization service as the advanced service
// The base service already contains all advanced functionality
export default visualizationService;
