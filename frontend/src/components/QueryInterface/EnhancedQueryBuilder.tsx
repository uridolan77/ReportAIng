import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Card,
  Input,
  Button,
  Typography,
  Alert,
  Spin,
  Tag,
  AutoComplete,
  Row,
  Col,
  Collapse,
  Tabs,
  List,
  Tooltip,
  Space,
  Flex,
} from 'antd';
import {
  SendOutlined as SendIcon,
  DownOutlined as ExpandMoreIcon,
  ClockCircleOutlined as TimerIcon,
  TableOutlined as TableRowsIcon,
  BulbOutlined as PsychologyIcon,
  AppstoreOutlined as CategoryIcon,
  RiseOutlined as TrendingUpIcon,
  BulbOutlined as LightbulbIcon,
  SwapOutlined as CompareIcon,
  DashboardOutlined as SpeedIcon,
  StarOutlined as StarIcon,
  WarningOutlined as WarningIcon,
} from '@ant-design/icons';
import {
  ApiService,
  EnhancedQueryRequest,
  EnhancedQueryResponse,
  SemanticAnalysisResponse,
  ClassificationResponse,
} from '../../services/api';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`enhanced-tabpanel-${index}`}
      aria-labelledby={`enhanced-tab-${index}`}
      {...other}
    >
      {value === index && <div style={{ padding: 24 }}>{children}</div>}
    </div>
  );
}

const EnhancedQueryBuilder: React.FC = () => {
  const [query, setQuery] = useState('');
  const [result, setResult] = useState<EnhancedQueryResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [loadingSuggestions, setLoadingSuggestions] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [analyzing, setAnalyzing] = useState(false);
  const [semanticAnalysis, setSemanticAnalysis] = useState<SemanticAnalysisResponse | null>(null);
  const [classification, setClassification] = useState<ClassificationResponse | null>(null);

  // Enhanced sample queries with AI insights - memoized to prevent re-creation
  const sampleQueries = useMemo(() => [
    "Show me total sales by month for this year",
    "What are the top 10 customers by revenue?",
    "Analyze customer behavior trends over the last quarter",
    "Compare sales performance between regions",
    "Show me products with declining sales trends",
    "What's the customer retention rate by segment?",
    "Find customers at risk of churning",
    "Show me the most profitable product categories",
  ], []);

  const loadEnhancedSuggestions = useCallback(async () => {
    try {
      setLoadingSuggestions(true);
      const apiSuggestions = await ApiService.getEnhancedQuerySuggestions();
      setSuggestions([...sampleQueries, ...apiSuggestions]);
    } catch (err) {
      console.error('Failed to load enhanced suggestions:', err);
      setSuggestions(sampleQueries);
    } finally {
      setLoadingSuggestions(false);
    }
  }, [sampleQueries]);

  useEffect(() => {
    loadEnhancedSuggestions();
  }, [loadEnhancedSuggestions]);

  const handleAnalyzeQuery = async () => {
    if (!query.trim()) return;

    setAnalyzing(true);
    try {
      const [analysisResult, classificationResult] = await Promise.all([
        ApiService.analyzeQuery(query),
        ApiService.classifyQuery(query),
      ]);

      setSemanticAnalysis(analysisResult);
      setClassification(classificationResult);
    } catch (err) {
      console.error('Failed to analyze query:', err);
    } finally {
      setAnalyzing(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!query.trim()) return;

    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const request: EnhancedQueryRequest = {
        query: query,
        executeQuery: true,
        includeAlternatives: true,
        includeSemanticAnalysis: true,
      };

      const queryResult = await ApiService.processEnhancedQuery(request);
      setResult(queryResult);

      if (!queryResult.success) {
        setError(queryResult.errorMessage || 'Query execution failed');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'An error occurred while executing the query');
    } finally {
      setLoading(false);
    }
  };

  const handleSuggestionSelect = (suggestion: string | null) => {
    if (suggestion) {
      setQuery(suggestion);
    }
  };

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return 'success';
    if (confidence >= 0.6) return 'warning';
    return 'error';
  };

  const getComplexityColor = (complexity: string) => {
    switch (complexity.toLowerCase()) {
      case 'low': return 'success';
      case 'medium': return 'warning';
      case 'high': return 'error';
      default: return 'default';
    }
  };

  const renderSemanticAnalysis = () => {
    if (!semanticAnalysis) return null;

    return (
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            <PsychologyIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
            Semantic Analysis
          </Typography>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle2" gutterBottom>Intent</Typography>
              <Chip
                label={semanticAnalysis.intent}
                color="primary"
                icon={<CategoryIcon />}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle2" gutterBottom>Confidence</Typography>
              <Chip
                label={`${(semanticAnalysis.confidence * 100).toFixed(1)}%`}
                color={getConfidenceColor(semanticAnalysis.confidence)}
                icon={<SpeedIcon />}
              />
            </Grid>
          </Grid>

          {semanticAnalysis.entities.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2" gutterBottom>Detected Entities</Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {semanticAnalysis.entities.map((entity, index) => (
                  <Tooltip key={index} title={`Type: ${entity.type}, Confidence: ${(entity.confidence * 100).toFixed(1)}%`}>
                    <Chip
                      label={entity.text}
                      size="small"
                      variant="outlined"
                      color={getConfidenceColor(entity.confidence)}
                    />
                  </Tooltip>
                ))}
              </Box>
            </Box>
          )}

          {semanticAnalysis.keywords.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2" gutterBottom>Keywords</Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {semanticAnalysis.keywords.map((keyword, index) => (
                  <Chip key={index} label={keyword} size="small" />
                ))}
              </Box>
            </Box>
          )}
        </CardContent>
      </Card>
    );
  };

  const renderClassification = () => {
    if (!classification) return null;

    return (
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            <CategoryIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
            Query Classification
          </Typography>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 4 }}>
              <Typography variant="subtitle2" gutterBottom>Category</Typography>
              <Chip label={classification.category} color="primary" />
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <Typography variant="subtitle2" gutterBottom>Complexity</Typography>
              <Chip
                label={classification.complexity}
                color={getComplexityColor(classification.complexity)}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <Typography variant="subtitle2" gutterBottom>Recommended Visualization</Typography>
              <Chip label={classification.recommendedVisualization} color="info" />
            </Grid>
          </Grid>

          {classification.predictedTables.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2" gutterBottom>Predicted Tables</Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {classification.predictedTables.map((table, index) => (
                  <Chip key={index} label={table} size="small" variant="outlined" />
                ))}
              </Box>
            </Box>
          )}

          {classification.optimizationSuggestions.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                <LightbulbIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Optimization Suggestions
              </Typography>
              <List dense>
                {classification.optimizationSuggestions.map((suggestion, index) => (
                  <ListItem key={index}>
                    <ListItemIcon>
                      <TrendingUpIcon color="primary" />
                    </ListItemIcon>
                    <ListItemText primary={suggestion} />
                  </ListItem>
                ))}
              </List>
            </Box>
          )}
        </CardContent>
      </Card>
    );
  };

  const renderAlternatives = () => {
    if (!result?.alternatives || result.alternatives.length === 0) return null;

    return (
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            <CompareIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
            Alternative Queries
          </Typography>

          {result.alternatives.map((alternative, index) => (
            <Accordion key={index}>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', width: '100%' }}>
                  <Typography variant="subtitle1" sx={{ flexGrow: 1 }}>
                    Alternative {index + 1}
                  </Typography>
                  <Chip
                    label={`Score: ${(alternative.score * 100).toFixed(1)}%`}
                    size="small"
                    color={getConfidenceColor(alternative.score)}
                    sx={{ mr: 1 }}
                  />
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Typography variant="body2" gutterBottom>
                  <strong>Reasoning:</strong> {alternative.reasoning}
                </Typography>

                <Paper sx={{ p: 2, bgcolor: 'grey.100', mb: 2 }}>
                  <Typography variant="body2" component="pre" sx={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap' }}>
                    {alternative.sql}
                  </Typography>
                </Paper>

                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" gutterBottom color="success.main">
                      <StarIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                      Strengths
                    </Typography>
                    <List dense>
                      {alternative.strengths.map((strength, idx) => (
                        <ListItem key={idx}>
                          <ListItemText primary={strength} />
                        </ListItem>
                      ))}
                    </List>
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" gutterBottom color="warning.main">
                      <WarningIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                      Considerations
                    </Typography>
                    <List dense>
                      {alternative.weaknesses.map((weakness, idx) => (
                        <ListItem key={idx}>
                          <ListItemText primary={weakness} />
                        </ListItem>
                      ))}
                    </List>
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>
          ))}
        </CardContent>
      </Card>
    );
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Enhanced AI Query Builder
      </Typography>
      <Typography variant="body1" color="text.secondary" gutterBottom>
        Ask questions about your data with advanced AI analysis and insights
      </Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box component="form" onSubmit={handleSubmit}>
            <Autocomplete
              freeSolo
              options={suggestions}
              loading={loadingSuggestions}
              value={query}
              onInputChange={(event, newValue) => setQuery(newValue || '')}
              onChange={(event, newValue) => handleSuggestionSelect(newValue)}
              renderInput={(params) => (
                <TextField
                  {...params}
                  fullWidth
                  multiline
                  rows={3}
                  placeholder="Ask a question about your data with AI-powered insights... (e.g., 'Analyze customer behavior trends')"
                  variant="outlined"
                  sx={{ mb: 2 }}
                />
              )}
              renderOption={(props, option) => (
                <li {...props}>
                  <Typography variant="body2">{option}</Typography>
                </li>
              )}
            />

            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Button
                type="submit"
                variant="contained"
                startIcon={loading ? <CircularProgress size={20} /> : <SendIcon />}
                disabled={loading || !query.trim()}
                size="large"
              >
                {loading ? 'Processing...' : 'Execute Enhanced Query'}
              </Button>

              <Button
                variant="outlined"
                startIcon={analyzing ? <CircularProgress size={20} /> : <PsychologyIcon />}
                disabled={analyzing || !query.trim()}
                onClick={handleAnalyzeQuery}
                size="large"
              >
                {analyzing ? 'Analyzing...' : 'Analyze Query'}
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {typeof error === 'string' ? error : error?.message || 'An error occurred'}
        </Alert>
      )}

      {/* Analysis Results */}
      {(semanticAnalysis || classification) && (
        <Box sx={{ mb: 3 }}>
          {renderSemanticAnalysis()}
          {renderClassification()}
        </Box>
      )}

      {/* Enhanced Results */}
      {result && (
        <Box>
          <Tabs value={tabValue} onChange={(e, newValue) => setTabValue(newValue)} sx={{ mb: 2 }}>
            <Tab label="Results" />
            <Tab label="AI Insights" />
            <Tab label="Alternatives" />
          </Tabs>

          <TabPanel value={tabValue} index={0}>
            {result.queryResult && (
              <Box>
                <Grid container spacing={2} sx={{ mb: 2 }}>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <Chip
                      icon={<TimerIcon />}
                      label={`${result.queryResult.executionTimeMs}ms`}
                      color="primary"
                      variant="outlined"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <Chip
                      icon={<TableRowsIcon />}
                      label={`${result.queryResult.rowCount || result.queryResult.data?.length || 0} rows`}
                      color="success"
                      variant="outlined"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <Chip
                      label={`${result.queryResult.columns?.length || 0} columns`}
                      color="info"
                      variant="outlined"
                    />
                  </Grid>
                </Grid>

                {result.processedQuery?.sql && (
                  <Accordion sx={{ mb: 2 }}>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                      <Typography variant="subtitle1">
                        Generated SQL
                        <Chip
                          label={`${(result.processedQuery.confidence * 100).toFixed(1)}% confidence`}
                          size="small"
                          color={getConfidenceColor(result.processedQuery.confidence)}
                          sx={{ ml: 2 }}
                        />
                      </Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                      <Paper sx={{ p: 2, bgcolor: 'grey.100' }}>
                        <Typography variant="body2" component="pre" sx={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap' }}>
                          {result.processedQuery.sql}
                        </Typography>
                      </Paper>
                      {result.processedQuery.explanation && (
                        <Box sx={{ mt: 2 }}>
                          <Typography variant="subtitle2" gutterBottom>Explanation</Typography>
                          <Typography variant="body2">{result.processedQuery.explanation}</Typography>
                        </Box>
                      )}
                    </AccordionDetails>
                  </Accordion>
                )}
              </Box>
            )}
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            {result.semanticAnalysis && (
              <Card sx={{ mb: 2 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Semantic Analysis</Typography>
                  <Typography variant="body2">Intent: {result.semanticAnalysis.intent}</Typography>
                  <Typography variant="body2">Confidence: {(result.semanticAnalysis.confidence * 100).toFixed(1)}%</Typography>
                </CardContent>
              </Card>
            )}

            {result.classification && (
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Classification</Typography>
                  <Typography variant="body2">Category: {result.classification.category}</Typography>
                  <Typography variant="body2">Complexity: {result.classification.complexity}</Typography>
                  <Typography variant="body2">Recommended Visualization: {result.classification.recommendedVisualization}</Typography>
                </CardContent>
              </Card>
            )}
          </TabPanel>

          <TabPanel value={tabValue} index={2}>
            {renderAlternatives()}
          </TabPanel>
        </Box>
      )}
    </Box>
  );
};

export default EnhancedQueryBuilder;