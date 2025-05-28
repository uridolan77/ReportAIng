import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  Grid,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  LinearProgress,
  Divider,
  Paper,
} from '@mui/material';
import {
  Compare as CompareIcon,
  Psychology as PsychologyIcon,
  TrendingUp as TrendingUpIcon,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Lightbulb as LightbulbIcon,
} from '@mui/icons-material';
import { ApiService, SimilarityRequest, SimilarityResponse, SimilarQueryResponse } from '../../services/api';

const QuerySimilarityAnalyzer: React.FC = () => {
  const [query1, setQuery1] = useState('');
  const [query2, setQuery2] = useState('');
  const [similarityResult, setSimilarityResult] = useState<SimilarityResponse | null>(null);
  const [similarQueries, setSimilarQueries] = useState<SimilarQueryResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingSimilar, setLoadingSimilar] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCompareSimilarity = async () => {
    if (!query1.trim() || !query2.trim()) {
      setError('Please enter both queries to compare');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const request: SimilarityRequest = {
        query1: query1.trim(),
        query2: query2.trim(),
      };

      const result = await ApiService.calculateSimilarity(request);
      setSimilarityResult(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to calculate similarity');
    } finally {
      setLoading(false);
    }
  };

  const handleFindSimilar = async () => {
    if (!query1.trim()) {
      setError('Please enter a query to find similar ones');
      return;
    }

    setLoadingSimilar(true);
    setError(null);

    try {
      const result = await ApiService.findSimilarQueries(query1.trim(), 5);
      setSimilarQueries(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to find similar queries');
    } finally {
      setLoadingSimilar(false);
    }
  };

  const getSimilarityColor = (score: number) => {
    if (score >= 0.8) return 'success';
    if (score >= 0.6) return 'warning';
    if (score >= 0.4) return 'info';
    return 'error';
  };

  const getSimilarityIcon = (score: number) => {
    if (score >= 0.8) return <CheckCircleIcon color="success" />;
    if (score >= 0.6) return <WarningIcon color="warning" />;
    return <ErrorIcon color="error" />;
  };

  const getSimilarityDescription = (score: number) => {
    if (score >= 0.9) return 'Nearly Identical';
    if (score >= 0.8) return 'Very Similar';
    if (score >= 0.6) return 'Similar';
    if (score >= 0.4) return 'Somewhat Similar';
    if (score >= 0.2) return 'Slightly Similar';
    return 'Very Different';
  };

  const sampleQueries = [
    "Show me total sales by month for this year",
    "What are the top 10 customers by revenue?",
    "Display monthly sales totals for current year",
    "List the highest revenue customers",
    "Analyze customer behavior trends over the last quarter",
    "Show me products with declining sales trends",
  ];

  return (
    <Box>
      <Typography variant="h5" gutterBottom>
        <CompareIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
        Query Similarity Analyzer
      </Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Compare queries semantically and find similar patterns in your query history
      </Typography>

      {/* Query Comparison */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Compare Two Queries
          </Typography>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                multiline
                rows={3}
                label="First Query"
                value={query1}
                onChange={(e) => setQuery1(e.target.value)}
                placeholder="Enter your first query..."
                variant="outlined"
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                multiline
                rows={3}
                label="Second Query"
                value={query2}
                onChange={(e) => setQuery2(e.target.value)}
                placeholder="Enter your second query..."
                variant="outlined"
              />
            </Grid>
          </Grid>

          <Box sx={{ mt: 2, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Button
              variant="contained"
              startIcon={loading ? <CircularProgress size={20} /> : <CompareIcon />}
              onClick={handleCompareSimilarity}
              disabled={loading || !query1.trim() || !query2.trim()}
            >
              {loading ? 'Comparing...' : 'Compare Similarity'}
            </Button>

            <Button
              variant="outlined"
              startIcon={loadingSimilar ? <CircularProgress size={20} /> : <PsychologyIcon />}
              onClick={handleFindSimilar}
              disabled={loadingSimilar || !query1.trim()}
            >
              {loadingSimilar ? 'Finding...' : 'Find Similar Queries'}
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Sample Queries */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            <LightbulbIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
            Sample Queries to Try
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
            {sampleQueries.map((sample, index) => (
              <Chip
                key={index}
                label={sample}
                variant="outlined"
                onClick={() => {
                  if (!query1.trim()) {
                    setQuery1(sample);
                  } else if (!query2.trim()) {
                    setQuery2(sample);
                  } else {
                    setQuery1(sample);
                    setQuery2('');
                  }
                }}
                sx={{ cursor: 'pointer' }}
              />
            ))}
          </Box>
        </CardContent>
      </Card>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Similarity Results */}
      {similarityResult && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Similarity Analysis Results
            </Typography>

            <Box sx={{ mb: 3 }}>
              <Box display="flex" alignItems="center" mb={2}>
                {getSimilarityIcon(similarityResult.similarityScore)}
                <Typography variant="h4" sx={{ ml: 1, mr: 2 }}>
                  {(similarityResult.similarityScore * 100).toFixed(1)}%
                </Typography>
                <Chip
                  label={getSimilarityDescription(similarityResult.similarityScore)}
                  color={getSimilarityColor(similarityResult.similarityScore)}
                />
              </Box>

              <LinearProgress
                variant="determinate"
                value={similarityResult.similarityScore * 100}
                color={getSimilarityColor(similarityResult.similarityScore)}
                sx={{ height: 8, borderRadius: 4 }}
              />
            </Box>

            <Typography variant="body1" gutterBottom>
              <strong>Analysis:</strong> {similarityResult.analysis}
            </Typography>

            <Grid container spacing={2} sx={{ mt: 2 }}>
              {similarityResult.commonEntities.length > 0 && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" gutterBottom>
                    Common Entities
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    {similarityResult.commonEntities.map((entity, index) => (
                      <Chip
                        key={index}
                        label={entity}
                        size="small"
                        color="primary"
                        variant="outlined"
                      />
                    ))}
                  </Box>
                </Grid>
              )}

              {similarityResult.commonKeywords.length > 0 && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" gutterBottom>
                    Common Keywords
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    {similarityResult.commonKeywords.map((keyword, index) => (
                      <Chip
                        key={index}
                        label={keyword}
                        size="small"
                        color="secondary"
                        variant="outlined"
                      />
                    ))}
                  </Box>
                </Grid>
              )}
            </Grid>
          </CardContent>
        </Card>
      )}

      {/* Similar Queries */}
      {similarQueries.length > 0 && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              <TrendingUpIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Similar Queries Found
            </Typography>

            <List>
              {similarQueries.map((similar, index) => (
                <React.Fragment key={index}>
                  <ListItem>
                    <ListItemIcon>
                      <PsychologyIcon color="primary" />
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Box display="flex" alignItems="center" justifyContent="space-between">
                          <Typography variant="body1" sx={{ flexGrow: 1 }}>
                            {similar.explanation}
                          </Typography>
                          <Box display="flex" gap={1}>
                            <Chip
                              label={`${(similar.confidence * 100).toFixed(1)}%`}
                              size="small"
                              color={getSimilarityColor(similar.confidence)}
                            />
                            <Chip
                              label={similar.classification}
                              size="small"
                              variant="outlined"
                            />
                          </Box>
                        </Box>
                      }
                      secondary={
                        <Paper sx={{ p: 1, mt: 1, bgcolor: 'grey.100' }}>
                          <Typography
                            variant="caption"
                            component="pre"
                            sx={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap' }}
                          >
                            {similar.sql}
                          </Typography>
                        </Paper>
                      }
                    />
                  </ListItem>
                  {index < similarQueries.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          </CardContent>
        </Card>
      )}

      {/* Empty State */}
      {!similarityResult && similarQueries.length === 0 && !loading && !loadingSimilar && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <CompareIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              Analyze Query Similarity
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Enter queries above to compare their semantic similarity or find similar patterns in your history.
            </Typography>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default QuerySimilarityAnalyzer;
