AIPipelineTestPage.tsx:180 🔗 Joined test session: test_1750938460714_ja659dark
AIPipelineTestPage.tsx:191 🚀 Sending test request: {testId: 'test_1750938460714_ja659dark', query: 'Top 10 depositors yesterday from UK', steps: Array(8), parameters: {…}}
AIPipelineTestPage.tsx:192 🔍 [FRONTEND-DEBUG] Frontend code updated - step data merging enabled
AIPipelineTestPage.tsx:193 Warning: [antd: message] Static function can not consume context like dynamic theme. Please use 'App' component instead.
warning @ chunk-L4GHDMH2.js?v=abb20976:1005
call @ chunk-L4GHDMH2.js?v=abb20976:1024
warningOnce @ chunk-L4GHDMH2.js?v=abb20976:1029
warning2 @ antd.js?v=abb20976:3340
warnContext @ antd.js?v=abb20976:6252
typeOpen @ antd.js?v=abb20976:69450
staticMethods.<computed> @ antd.js?v=abb20976:69493
runTest @ AIPipelineTestPage.tsx:193
await in runTest
(anonymous) @ antd.js?v=abb20976:13551
callCallback2 @ chunk-XQLYTHWV.js?v=abb20976:3674
invokeGuardedCallbackDev @ chunk-XQLYTHWV.js?v=abb20976:3699
invokeGuardedCallback @ chunk-XQLYTHWV.js?v=abb20976:3733
invokeGuardedCallbackAndCatchFirstError @ chunk-XQLYTHWV.js?v=abb20976:3736
executeDispatch @ chunk-XQLYTHWV.js?v=abb20976:7014
processDispatchQueueItemsInOrder @ chunk-XQLYTHWV.js?v=abb20976:7034
processDispatchQueue @ chunk-XQLYTHWV.js?v=abb20976:7043
dispatchEventsForPlugins @ chunk-XQLYTHWV.js?v=abb20976:7051
(anonymous) @ chunk-XQLYTHWV.js?v=abb20976:7174
batchedUpdates$1 @ chunk-XQLYTHWV.js?v=abb20976:18913
batchedUpdates @ chunk-XQLYTHWV.js?v=abb20976:3579
dispatchEventForPluginEventSystem @ chunk-XQLYTHWV.js?v=abb20976:7173
dispatchEventWithEnableCapturePhaseSelectiveHydrationWithoutDiscreteEventReplay @ chunk-XQLYTHWV.js?v=abb20976:5478
dispatchEvent @ chunk-XQLYTHWV.js?v=abb20976:5472
dispatchDiscreteEvent @ chunk-XQLYTHWV.js?v=abb20976:5449
AIPipelineTestPage.tsx:196 ✅ Received test result: {testId: 'test_1750938460714_ja659dark', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:47:40.7993871Z', endTime: '2025-06-26T11:47:46.6950787Z', …}
AIPipelineTestPage.tsx:197 🔍 Setting testResult state...
AIPipelineTestPage.tsx:204 🔍 Current stepProgress state: {}
AIPipelineTestPage.tsx:205 🔍 stepProgress keys: []
AIPipelineTestPage.tsx:246 ⚠️ No stepProgress data available for merging
AIPipelineTestPage.tsx:251 ✅ State updated, testResult should now be: {testId: 'test_1750938460714_ja659dark', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:47:40.7993871Z', endTime: '2025-06-26T11:47:46.6950787Z', …}
AIPipelineTestResults.tsx:44 🔍 AIPipelineTestResults rendering with result: {testId: 'test_1750938460714_ja659dark', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:47:40.7993871Z', endTime: '2025-06-26T11:47:46.6950787Z', …}
AIPipelineTestResults.tsx:45 🔍 Available step results: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:46 🔍 AIGeneration step data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:137 🔥🔥🔥 renderStepResults CALLED! 🔥🔥🔥
AIPipelineTestResults.tsx:138 🔍 Rendering step results. requestedSteps: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:139 🔍 Available results: {BusinessContextAnalysis: {…}, TokenBudgetManagement: {…}, SchemaRetrieval: {…}, PromptBuilding: {…}, AIGeneration: {…}, …}
AIPipelineTestResults.tsx:140 🔍 result.results type: object
AIPipelineTestResults.tsx:141 🔍 result.results keys: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:145 🔍 Step BusinessContextAnalysis: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:146 🔍 Step BusinessContextAnalysis has data: true
AIPipelineTestResults.tsx:145 🔍 Step TokenBudgetManagement: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step TokenBudgetManagement has data: true
AIPipelineTestResults.tsx:145 🔍 Step SchemaRetrieval: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SchemaRetrieval has data: true
AIPipelineTestResults.tsx:145 🔍 Step PromptBuilding: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 1834, …}
AIPipelineTestResults.tsx:146 🔍 Step PromptBuilding has data: true
AIPipelineTestResults.tsx:145 🔍 Step AIGeneration: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:146 🔍 Step AIGeneration has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLValidation: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLValidation has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLExecution: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLExecution has data: true
AIPipelineTestResults.tsx:145 🔍 Step ResultsProcessing: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:146 🔍 Step ResultsProcessing has data: true
AIPipelineTestResults.tsx:157 🔍 Processing step BusinessContextAnalysis, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for BusinessContextAnalysis with data: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for BusinessContextAnalysis: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step TokenBudgetManagement, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for TokenBudgetManagement with data: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for TokenBudgetManagement: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SchemaRetrieval, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SchemaRetrieval with data: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SchemaRetrieval: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step PromptBuilding, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for PromptBuilding with data: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 1834, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for PromptBuilding: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step AIGeneration, hasData: ```SQL
SELECT PlayerID, SUM(DepositAmount) AS TotalDeposit
FROM common.tbl_Daily_actions
WHERE Date = DATE_SUB(CURDATE(), INTERVAL 1 DAY) AND Country = 'UK'
GROUP BY PlayerID
ORDER BY TotalDeposit DESC
LIMIT 10;
```
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for AIGeneration with data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:420 🎯 [AI-CASE] AIGeneration case entered! stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:423 🤖 [AI-CONDITION] stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:424 🤖 [AI-CONDITION] !stepResult: false
AIPipelineTestResults.tsx:425 🤖 [AI-CONDITION] !stepResult.generatedSQL: false
AIPipelineTestResults.tsx:426 🤖 [AI-CONDITION] !stepResult.sqlLength: false
AIPipelineTestResults.tsx:427 🤖 [AI-CONDITION] stepResult.success === undefined: false
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for AIGeneration: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLValidation, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLValidation with data: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLValidation: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLExecution, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLExecution with data: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLExecution: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step ResultsProcessing, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for ResultsProcessing with data: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for ResultsProcessing: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:212 🔍 Steps with data: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:213 🔍 Setting default active keys (no duplicates): (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:44 🔍 AIPipelineTestResults rendering with result: {testId: 'test_1750938460714_ja659dark', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:47:40.7993871Z', endTime: '2025-06-26T11:47:46.6950787Z', …}
AIPipelineTestResults.tsx:45 🔍 Available step results: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:46 🔍 AIGeneration step data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:137 🔥🔥🔥 renderStepResults CALLED! 🔥🔥🔥
AIPipelineTestResults.tsx:138 🔍 Rendering step results. requestedSteps: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:139 🔍 Available results: {BusinessContextAnalysis: {…}, TokenBudgetManagement: {…}, SchemaRetrieval: {…}, PromptBuilding: {…}, AIGeneration: {…}, …}
AIPipelineTestResults.tsx:140 🔍 result.results type: object
AIPipelineTestResults.tsx:141 🔍 result.results keys: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:145 🔍 Step BusinessContextAnalysis: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:146 🔍 Step BusinessContextAnalysis has data: true
AIPipelineTestResults.tsx:145 🔍 Step TokenBudgetManagement: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step TokenBudgetManagement has data: true
AIPipelineTestResults.tsx:145 🔍 Step SchemaRetrieval: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SchemaRetrieval has data: true
AIPipelineTestResults.tsx:145 🔍 Step PromptBuilding: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 1834, …}
AIPipelineTestResults.tsx:146 🔍 Step PromptBuilding has data: true
AIPipelineTestResults.tsx:145 🔍 Step AIGeneration: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:146 🔍 Step AIGeneration has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLValidation: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLValidation has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLExecution: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLExecution has data: true
AIPipelineTestResults.tsx:145 🔍 Step ResultsProcessing: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:146 🔍 Step ResultsProcessing has data: true
AIPipelineTestResults.tsx:157 🔍 Processing step BusinessContextAnalysis, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for BusinessContextAnalysis with data: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for BusinessContextAnalysis: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step TokenBudgetManagement, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for TokenBudgetManagement with data: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for TokenBudgetManagement: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SchemaRetrieval, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SchemaRetrieval with data: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SchemaRetrieval: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step PromptBuilding, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for PromptBuilding with data: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 1834, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for PromptBuilding: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step AIGeneration, hasData: ```SQL
SELECT PlayerID, SUM(DepositAmount) AS TotalDeposit
FROM common.tbl_Daily_actions
WHERE Date = DATE_SUB(CURDATE(), INTERVAL 1 DAY) AND Country = 'UK'
GROUP BY PlayerID
ORDER BY TotalDeposit DESC
LIMIT 10;
```
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for AIGeneration with data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:420 🎯 [AI-CASE] AIGeneration case entered! stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:423 🤖 [AI-CONDITION] stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3220, …}
AIPipelineTestResults.tsx:424 🤖 [AI-CONDITION] !stepResult: false
AIPipelineTestResults.tsx:425 🤖 [AI-CONDITION] !stepResult.generatedSQL: false
AIPipelineTestResults.tsx:426 🤖 [AI-CONDITION] !stepResult.sqlLength: false
AIPipelineTestResults.tsx:427 🤖 [AI-CONDITION] stepResult.success === undefined: false
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for AIGeneration: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLValidation, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLValidation with data: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLValidation: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLExecution, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLExecution with data: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLExecution: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step ResultsProcessing, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for ResultsProcessing with data: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for ResultsProcessing: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:212 🔍 Steps with data: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:213 🔍 Setting default active keys (no duplicates): (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestPage.tsx:114 🔍 testResult state changed: {testId: 'test_1750938460714_ja659dark', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:47:40.7993871Z', endTime: '2025-06-26T11:47:46.6950787Z', …}
AIPipelineTestPage.tsx:180 🔗 Joined test session: test_1750938796641_8g9johavp
AIPipelineTestPage.tsx:191 🚀 Sending test request: {testId: 'test_1750938796641_8g9johavp', query: 'Top 10 depositors yesterday from UK', steps: Array(8), parameters: {…}}
AIPipelineTestPage.tsx:192 🔍 [FRONTEND-DEBUG] Frontend code updated - step data merging enabled
AIPipelineTestPage.tsx:193 Warning: [antd: message] Static function can not consume context like dynamic theme. Please use 'App' component instead.
warning @ chunk-L4GHDMH2.js?v=abb20976:1005
call @ chunk-L4GHDMH2.js?v=abb20976:1024
warningOnce @ chunk-L4GHDMH2.js?v=abb20976:1029
warning2 @ antd.js?v=abb20976:3340
warnContext @ antd.js?v=abb20976:6252
typeOpen @ antd.js?v=abb20976:69450
staticMethods.<computed> @ antd.js?v=abb20976:69493
runTest @ AIPipelineTestPage.tsx:193
await in runTest
(anonymous) @ antd.js?v=abb20976:13551
callCallback2 @ chunk-XQLYTHWV.js?v=abb20976:3674
invokeGuardedCallbackDev @ chunk-XQLYTHWV.js?v=abb20976:3699
invokeGuardedCallback @ chunk-XQLYTHWV.js?v=abb20976:3733
invokeGuardedCallbackAndCatchFirstError @ chunk-XQLYTHWV.js?v=abb20976:3736
executeDispatch @ chunk-XQLYTHWV.js?v=abb20976:7014
processDispatchQueueItemsInOrder @ chunk-XQLYTHWV.js?v=abb20976:7034
processDispatchQueue @ chunk-XQLYTHWV.js?v=abb20976:7043
dispatchEventsForPlugins @ chunk-XQLYTHWV.js?v=abb20976:7051
(anonymous) @ chunk-XQLYTHWV.js?v=abb20976:7174
batchedUpdates$1 @ chunk-XQLYTHWV.js?v=abb20976:18913
batchedUpdates @ chunk-XQLYTHWV.js?v=abb20976:3579
dispatchEventForPluginEventSystem @ chunk-XQLYTHWV.js?v=abb20976:7173
dispatchEventWithEnableCapturePhaseSelectiveHydrationWithoutDiscreteEventReplay @ chunk-XQLYTHWV.js?v=abb20976:5478
dispatchEvent @ chunk-XQLYTHWV.js?v=abb20976:5472
dispatchDiscreteEvent @ chunk-XQLYTHWV.js?v=abb20976:5449
AIPipelineTestPage.tsx:196 ✅ Received test result: {testId: 'test_1750938796641_8g9johavp', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:53:16.7401954Z', endTime: '2025-06-26T11:53:22.902003Z', …}
AIPipelineTestPage.tsx:197 🔍 Setting testResult state...
AIPipelineTestPage.tsx:204 🔍 Current stepProgress state: {}
AIPipelineTestPage.tsx:205 🔍 stepProgress keys: []
AIPipelineTestPage.tsx:246 ⚠️ No stepProgress data available for merging
AIPipelineTestPage.tsx:251 ✅ State updated, testResult should now be: {testId: 'test_1750938796641_8g9johavp', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:53:16.7401954Z', endTime: '2025-06-26T11:53:22.902003Z', …}
AIPipelineTestResults.tsx:44 🔍 AIPipelineTestResults rendering with result: {testId: 'test_1750938796641_8g9johavp', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:53:16.7401954Z', endTime: '2025-06-26T11:53:22.902003Z', …}
AIPipelineTestResults.tsx:45 🔍 Available step results: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:46 🔍 AIGeneration step data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:137 🔥🔥🔥 renderStepResults CALLED! 🔥🔥🔥
AIPipelineTestResults.tsx:138 🔍 Rendering step results. requestedSteps: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:139 🔍 Available results: {BusinessContextAnalysis: {…}, TokenBudgetManagement: {…}, SchemaRetrieval: {…}, PromptBuilding: {…}, AIGeneration: {…}, …}
AIPipelineTestResults.tsx:140 🔍 result.results type: object
AIPipelineTestResults.tsx:141 🔍 result.results keys: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:145 🔍 Step BusinessContextAnalysis: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:146 🔍 Step BusinessContextAnalysis has data: true
AIPipelineTestResults.tsx:145 🔍 Step TokenBudgetManagement: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step TokenBudgetManagement has data: true
AIPipelineTestResults.tsx:145 🔍 Step SchemaRetrieval: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SchemaRetrieval has data: true
AIPipelineTestResults.tsx:145 🔍 Step PromptBuilding: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 2141, …}
AIPipelineTestResults.tsx:146 🔍 Step PromptBuilding has data: true
AIPipelineTestResults.tsx:145 🔍 Step AIGeneration: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:146 🔍 Step AIGeneration has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLValidation: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLValidation has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLExecution: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLExecution has data: true
AIPipelineTestResults.tsx:145 🔍 Step ResultsProcessing: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:146 🔍 Step ResultsProcessing has data: true
AIPipelineTestResults.tsx:157 🔍 Processing step BusinessContextAnalysis, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for BusinessContextAnalysis with data: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for BusinessContextAnalysis: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step TokenBudgetManagement, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for TokenBudgetManagement with data: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for TokenBudgetManagement: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SchemaRetrieval, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SchemaRetrieval with data: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SchemaRetrieval: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step PromptBuilding, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for PromptBuilding with data: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 2141, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for PromptBuilding: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step AIGeneration, hasData: ```SQL
SELECT PlayerID, SUM(DepositAmount) AS TotalDeposit
FROM common.tbl_Daily_actions
WHERE Date = DATE_SUB(CURDATE(), INTERVAL 1 DAY) AND Country = 'UK'
GROUP BY PlayerID
ORDER BY TotalDeposit DESC
LIMIT 10;
```
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for AIGeneration with data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:420 🎯 [AI-CASE] AIGeneration case entered! stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:423 🤖 [AI-CONDITION] stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:424 🤖 [AI-CONDITION] !stepResult: false
AIPipelineTestResults.tsx:425 🤖 [AI-CONDITION] !stepResult.generatedSQL: false
AIPipelineTestResults.tsx:426 🤖 [AI-CONDITION] !stepResult.sqlLength: false
AIPipelineTestResults.tsx:427 🤖 [AI-CONDITION] stepResult.success === undefined: false
AIPipelineTestResults.tsx:457 🎯 [AI-DETAILED-RENDER] About to render detailed AIGeneration content
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for AIGeneration: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLValidation, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLValidation with data: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLValidation: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLExecution, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLExecution with data: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLExecution: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step ResultsProcessing, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for ResultsProcessing with data: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for ResultsProcessing: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:212 🔍 Steps with data: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:213 🔍 Setting default active keys (no duplicates): (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:44 🔍 AIPipelineTestResults rendering with result: {testId: 'test_1750938796641_8g9johavp', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:53:16.7401954Z', endTime: '2025-06-26T11:53:22.902003Z', …}
AIPipelineTestResults.tsx:45 🔍 Available step results: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:46 🔍 AIGeneration step data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:137 🔥🔥🔥 renderStepResults CALLED! 🔥🔥🔥
AIPipelineTestResults.tsx:138 🔍 Rendering step results. requestedSteps: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:139 🔍 Available results: {BusinessContextAnalysis: {…}, TokenBudgetManagement: {…}, SchemaRetrieval: {…}, PromptBuilding: {…}, AIGeneration: {…}, …}
AIPipelineTestResults.tsx:140 🔍 result.results type: object
AIPipelineTestResults.tsx:141 🔍 result.results keys: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:145 🔍 Step BusinessContextAnalysis: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:146 🔍 Step BusinessContextAnalysis has data: true
AIPipelineTestResults.tsx:145 🔍 Step TokenBudgetManagement: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step TokenBudgetManagement has data: true
AIPipelineTestResults.tsx:145 🔍 Step SchemaRetrieval: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SchemaRetrieval has data: true
AIPipelineTestResults.tsx:145 🔍 Step PromptBuilding: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 2141, …}
AIPipelineTestResults.tsx:146 🔍 Step PromptBuilding has data: true
AIPipelineTestResults.tsx:145 🔍 Step AIGeneration: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:146 🔍 Step AIGeneration has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLValidation: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLValidation has data: true
AIPipelineTestResults.tsx:145 🔍 Step SQLExecution: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:146 🔍 Step SQLExecution has data: true
AIPipelineTestResults.tsx:145 🔍 Step ResultsProcessing: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:146 🔍 Step ResultsProcessing has data: true
AIPipelineTestResults.tsx:157 🔍 Processing step BusinessContextAnalysis, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for BusinessContextAnalysis with data: {businessProfile: {…}, extractedEntities: 4, confidenceScore: 0.91, intent: 'Aggregation', domain: 'Gaming', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for BusinessContextAnalysis: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step TokenBudgetManagement, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for TokenBudgetManagement with data: {tokenBudget: {…}, maxTokens: 4000, availableContextTokens: 3244, reservedTokens: 500, success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for TokenBudgetManagement: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SchemaRetrieval, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SchemaRetrieval with data: {schemaMetadata: {…}, tablesRetrieved: 3, relevanceScore: 0.8, tableNames: Array(3), success: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SchemaRetrieval: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step PromptBuilding, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for PromptBuilding with data: {prompt: 'You are an expert business intelligence analyst wi…y the SQL query without additional explanation.\r\n', promptLength: 1922, estimatedTokens: 481, success: true, durationMs: 2141, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for PromptBuilding: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step AIGeneration, hasData: ```SQL
SELECT PlayerID, SUM(DepositAmount) AS TotalDeposit
FROM common.tbl_Daily_actions
WHERE Date = DATE_SUB(CURDATE(), INTERVAL 1 DAY) AND Country = 'UK'
GROUP BY PlayerID
ORDER BY TotalDeposit DESC
LIMIT 10;
```
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for AIGeneration with data: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:420 🎯 [AI-CASE] AIGeneration case entered! stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:423 🤖 [AI-CONDITION] stepResult: {generatedSQL: '```SQL\nSELECT PlayerID, SUM(DepositAmount) AS Tota…PlayerID\nORDER BY TotalDeposit DESC\nLIMIT 10;\n```', sqlLength: 215, estimatedCost: 0.01767, success: true, durationMs: 3275, …}
AIPipelineTestResults.tsx:424 🤖 [AI-CONDITION] !stepResult: false
AIPipelineTestResults.tsx:425 🤖 [AI-CONDITION] !stepResult.generatedSQL: false
AIPipelineTestResults.tsx:426 🤖 [AI-CONDITION] !stepResult.sqlLength: false
AIPipelineTestResults.tsx:427 🤖 [AI-CONDITION] stepResult.success === undefined: false
AIPipelineTestResults.tsx:457 🎯 [AI-DETAILED-RENDER] About to render detailed AIGeneration content
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for AIGeneration: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLValidation, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLValidation with data: {isValid: false, validationErrors: Array(0), securityScore: 0.9, syntaxScore: 0.1, semanticScore: 0.8, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLValidation: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step SQLExecution, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for SQLExecution with data: {executedSuccessfully: true, rowsReturned: 0, executionTimeMs: 0, resultPreview: 'Actual execution disabled for safety', isSimulated: true, …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for SQLExecution: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:157 🔍 Processing step ResultsProcessing, hasData: true
AIPipelineTestResults.tsx:177 🎯 [RENDER-CALL] Calling renderStepSpecificResults for ResultsProcessing with data: {totalSteps: 7, successfulSteps: 7, successRate: 1, formattedResults: 'JSON format results generated', exportFormat: 'json', …}
AIPipelineTestResults.tsx:179 🎯 [RENDER-RESULT] Result for ResultsProcessing: {$$typeof: Symbol(react.element), type: 'div', key: null, ref: null, props: {…}, …}
AIPipelineTestResults.tsx:212 🔍 Steps with data: (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestResults.tsx:213 🔍 Setting default active keys (no duplicates): (8) ['BusinessContextAnalysis', 'TokenBudgetManagement', 'SchemaRetrieval', 'PromptBuilding', 'AIGeneration', 'SQLValidation', 'SQLExecution', 'ResultsProcessing']
AIPipelineTestPage.tsx:114 🔍 testResult state changed: {testId: 'test_1750938796641_8g9johavp', query: 'Top 10 depositors yesterday from UK', requestedSteps: Array(8), startTime: '2025-06-26T11:53:16.7401954Z', endTime: '2025-06-26T11:53:22.902003Z', …}
usePipelineTestMonitoring.ts:240 💓 Heartbeat response: {userId: 'admin-user-001', timestamp: '2025-06-26T11:53:30.669176Z'}
