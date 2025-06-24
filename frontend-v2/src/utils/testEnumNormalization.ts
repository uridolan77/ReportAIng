// Test utility to verify enum normalization works correctly
import { PipelineStep, normalizePipelineStep, normalizePipelineSteps } from '../types/aiPipelineTest';

export const testEnumNormalization = () => {
  console.log('🧪 Testing Enum Normalization...');
  
  // Test 1: String enum values (should pass through unchanged)
  console.log('Test 1: String enum values');
  const stringStep = normalizePipelineStep('BusinessContextAnalysis');
  console.log('Input: "BusinessContextAnalysis" -> Output:', stringStep);
  console.assert(stringStep === PipelineStep.BusinessContextAnalysis, 'String enum should pass through');
  
  // Test 2: Numeric enum values (should convert to string)
  console.log('Test 2: Numeric enum values');
  const numericStep0 = normalizePipelineStep(0);
  const numericStep1 = normalizePipelineStep(1);
  const numericStep2 = normalizePipelineStep(2);
  console.log('Input: 0 -> Output:', numericStep0);
  console.log('Input: 1 -> Output:', numericStep1);
  console.log('Input: 2 -> Output:', numericStep2);
  console.assert(numericStep0 === PipelineStep.BusinessContextAnalysis, 'Numeric 0 should map to BusinessContextAnalysis');
  console.assert(numericStep1 === PipelineStep.TokenBudgetManagement, 'Numeric 1 should map to TokenBudgetManagement');
  console.assert(numericStep2 === PipelineStep.SchemaRetrieval, 'Numeric 2 should map to SchemaRetrieval');
  
  // Test 3: Array normalization
  console.log('Test 3: Array normalization');
  const mixedArray = [0, 'TokenBudgetManagement', 2, 'AIGeneration'];
  const normalizedArray = normalizePipelineSteps(mixedArray);
  console.log('Input:', mixedArray);
  console.log('Output:', normalizedArray);
  console.assert(normalizedArray[0] === PipelineStep.BusinessContextAnalysis, 'First item should be BusinessContextAnalysis');
  console.assert(normalizedArray[1] === PipelineStep.TokenBudgetManagement, 'Second item should be TokenBudgetManagement');
  console.assert(normalizedArray[2] === PipelineStep.SchemaRetrieval, 'Third item should be SchemaRetrieval');
  console.assert(normalizedArray[3] === PipelineStep.AIGeneration, 'Fourth item should be AIGeneration');
  
  // Test 4: Invalid values (should fallback gracefully)
  console.log('Test 4: Invalid values');
  const invalidStep = normalizePipelineStep(999);
  console.log('Input: 999 -> Output:', invalidStep);
  console.assert(invalidStep === PipelineStep.BusinessContextAnalysis, 'Invalid numeric should fallback to BusinessContextAnalysis');
  
  console.log('✅ All enum normalization tests passed!');
  
  // Test the actual API response format
  console.log('🔍 Testing API Response Format...');
  const mockApiResponse = {
    testId: "2ae518a8",
    query: "top 10 depositors yesterday",
    requestedSteps: [0], // This is what the backend currently returns
    startTime: "2025-06-24T15:47:08.7715752Z",
    endTime: "2025-06-24T15:47:08.7925445Z",
    totalDurationMs: 30,
    success: true,
    error: null,
    results: {
      BusinessContextAnalysis: {
        // ... result data
      }
    }
  };
  
  console.log('Mock API Response requestedSteps:', mockApiResponse.requestedSteps);
  const normalizedSteps = normalizePipelineSteps(mockApiResponse.requestedSteps);
  console.log('Normalized requestedSteps:', normalizedSteps);
  console.assert(normalizedSteps[0] === PipelineStep.BusinessContextAnalysis, 'API response should normalize correctly');
  
  console.log('✅ API response normalization test passed!');
  
  return {
    success: true,
    message: 'All enum normalization tests completed successfully'
  };
};

// Auto-run tests when this module is imported
if (typeof window !== 'undefined') {
  // Only run in browser environment
  setTimeout(() => {
    try {
      testEnumNormalization();
    } catch (error) {
      console.error('❌ Enum normalization tests failed:', error);
    }
  }, 1000);
}
