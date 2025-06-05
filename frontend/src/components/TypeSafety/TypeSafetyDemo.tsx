import React, { useState } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Input,
  Alert,
  Typography,
  Space,
  Tag,
  Collapse,
  message,
  Form,
  Select,
  Switch
} from 'antd';
import {
  CheckCircleOutlined,
  WarningOutlined,
  CodeOutlined,
  BugOutlined,
  SafetyOutlined
} from '@ant-design/icons';
import { z } from 'zod';
import { ValidationUtils } from '../../utils/validation';
import { assertType, validateWithFallback } from '../../utils/validation';
import {
  createUserId,
  createQueryId,
  createConfidenceScore,
  createEmailAddress,
  isEmailAddress,
  isConfidenceScore,
  UserId,
  QueryId,
  ConfidenceScore,
  EmailAddress
} from '../../types/branded';
import { useValidatedQuery, useExecuteQuery } from '../../hooks/useValidatedQuery';
import { QueryResponseSchema } from '../../schemas/api';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;
const { Panel } = Collapse;

// Demo schemas for testing
const UserSchema = z.object({
  id: z.string(),
  name: z.string().min(1, 'Name is required'),
  email: z.string().email('Invalid email format'),
  age: z.number().min(0).max(150),
  isActive: z.boolean(),
  preferences: z.object({
    theme: z.enum(['light', 'dark']),
    notifications: z.boolean(),
  }).optional(),
});

const QueryDataSchema = z.object({
  query: z.string().min(1, 'Query cannot be empty'),
  confidence: z.number().min(0).max(1),
  executionTime: z.number().min(0),
  rowCount: z.number().min(0),
});

export const TypeSafetyDemo: React.FC = () => {
  const [validationResults, setValidationResults] = useState<any[]>([]);
  const [testData, setTestData] = useState('{"id": "123", "name": "John Doe", "email": "john@example.com", "age": 30, "isActive": true}');
  const [brandedTypeDemo, setBrandedTypeDemo] = useState({
    userId: '',
    queryId: '',
    email: '',
    confidence: ''
  });

  // Example of validated query hook usage
  const { data: queryData, isLoading, error } = useValidatedQuery(
    ['demo', 'typeValidation'],
    async () => {
      // Simulate API response
      return {
        queryId: 'demo-123',
        sql: 'SELECT * FROM users',
        result: {
          data: [{ id: 1, name: 'Test User' }],
          columns: [
            { name: 'id', type: 'number', nullable: false },
            { name: 'name', type: 'string', nullable: false }
          ],
          metadata: {
            rowCount: 1,
            columnCount: 2,
            executionTimeMs: 150,
            fromCache: false
          }
        },
        explanation: 'This is a demo query',
        confidence: 0.95,
        executionTimeMs: 150,
        fromCache: false
      };
    },
    QueryResponseSchema,
    {
      validationContext: 'demo-query',
      enabled: false // Disabled by default for demo
    }
  );

  const executeQueryMutation = useExecuteQuery();

  // Test validation functions
  const runValidationTests = () => {
    const results: any[] = [];

    // Test 1: Valid data
    try {
      const validData = JSON.parse(testData);
      const result = ValidationUtils.validate(UserSchema, validData);
      results.push({
        test: 'Valid User Data',
        success: result.success,
        data: result.data,
        error: result.error?.message,
        type: 'success'
      });
    } catch (error) {
      results.push({
        test: 'Valid User Data',
        success: false,
        error: 'Invalid JSON',
        type: 'error'
      });
    }

    // Test 2: Invalid data
    const invalidData = { id: 123, name: '', email: 'invalid-email', age: -5, isActive: 'yes' };
    const invalidResult = ValidationUtils.validate(UserSchema, invalidData);
    results.push({
      test: 'Invalid User Data',
      success: invalidResult.success,
      data: invalidResult.data,
      error: invalidResult.error?.message,
      details: invalidResult.error?.details,
      type: 'error'
    });

    // Test 3: Fallback validation
    const fallbackResult = validateWithFallback(
      z.number().min(0).max(1),
      'invalid-number',
      0.5,
      'confidence-score'
    );
    results.push({
      test: 'Fallback Validation',
      success: true,
      data: fallbackResult,
      message: 'Used fallback value for invalid input',
      type: 'warning'
    });

    // Test 4: Array validation
    const arrayData = [
      { query: 'SELECT * FROM users', confidence: 0.9, executionTime: 100, rowCount: 5 },
      { query: '', confidence: 1.5, executionTime: -10, rowCount: 'invalid' }, // Invalid
      { query: 'SELECT COUNT(*)', confidence: 0.8, executionTime: 50, rowCount: 1 }
    ];
    const arrayResult = ValidationUtils.validateArray(QueryDataSchema, arrayData);
    results.push({
      test: 'Array Validation',
      success: arrayResult.success,
      data: arrayResult.data,
      error: arrayResult.error?.message,
      details: arrayResult.error?.details,
      type: arrayResult.success ? 'success' : 'warning'
    });

    setValidationResults(results);
  };

  // Test branded types
  const testBrandedTypes = () => {
    try {
      // Test valid branded types
      const userId = createUserId(brandedTypeDemo.userId);
      const queryId = createQueryId(brandedTypeDemo.queryId);
      const email = createEmailAddress(brandedTypeDemo.email);
      const confidence = createConfidenceScore(parseFloat(brandedTypeDemo.confidence));

      message.success('All branded types created successfully!');

      // Demonstrate type safety
      console.log('Branded types created:', { userId, queryId, email, confidence });

    } catch (error) {
      message.error(`Branded type creation failed: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  };

  // Test type guards
  const testTypeGuards = () => {
    const testValues = [
      { value: 'user@example.com', guard: isEmailAddress, type: 'Email' },
      { value: 'invalid-email', guard: isEmailAddress, type: 'Email' },
      { value: 0.75, guard: isConfidenceScore, type: 'Confidence Score' },
      { value: 1.5, guard: isConfidenceScore, type: 'Confidence Score' },
    ];

    testValues.forEach(({ value, guard, type }) => {
      const isValid = guard(value);
      message.info(`${type} "${value}": ${isValid ? 'Valid' : 'Invalid'}`);
    });
  };

  // Test form validation
  const [form] = Form.useForm();
  const handleFormSubmit = (values: any) => {
    const formResult = validateFormData(UserSchema, values, 'user-form');

    if (formResult.isValid) {
      message.success('Form validation passed!');
      console.log('Valid form data:', formResult.data);
    } else {
      message.error('Form validation failed!');
      console.log('Form errors:', formResult.errors);
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>
        <SafetyOutlined /> Type Safety & Validation Demo
      </Title>

      <Alert
        message="Enhanced Type Safety Features"
        description="This demo showcases Zod schema validation, branded types, runtime type checking, and validated API calls."
        type="info"
        showIcon
        style={{ marginBottom: '24px' }}
      />

      <Row gutter={[16, 16]}>
        {/* Schema Validation Demo */}
        <Col xs={24} lg={12}>
          <Card title="Schema Validation Demo" style={{ height: '500px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Test Data (JSON):</Text>
                <TextArea
                  value={testData}
                  onChange={(e) => setTestData(e.target.value)}
                  rows={4}
                  placeholder="Enter JSON data to validate"
                />
              </div>

              <Button
                type="primary"
                onClick={runValidationTests}
                icon={<CodeOutlined />}
                block
              >
                Run Validation Tests
              </Button>

              <div style={{ maxHeight: '200px', overflowY: 'auto' }}>
                {validationResults.map((result, index) => (
                  <Alert
                    key={index}
                    message={result.test}
                    description={
                      <div>
                        <div>Success: {result.success ? 'Yes' : 'No'}</div>
                        {result.error && <div>Error: {result.error}</div>}
                        {result.message && <div>Message: {result.message}</div>}
                      </div>
                    }
                    type={result.type}
                    showIcon
                    style={{ marginBottom: '8px' }}

                  />
                ))}
              </div>
            </Space>
          </Card>
        </Col>

        {/* Branded Types Demo */}
        <Col xs={24} lg={12}>
          <Card title="Branded Types Demo" style={{ height: '500px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Create Branded Types:</Text>
                <Input
                  placeholder="User ID"
                  value={brandedTypeDemo.userId}
                  onChange={(e) => setBrandedTypeDemo(prev => ({ ...prev, userId: e.target.value }))}
                  style={{ marginBottom: '8px' }}
                />
                <Input
                  placeholder="Query ID"
                  value={brandedTypeDemo.queryId}
                  onChange={(e) => setBrandedTypeDemo(prev => ({ ...prev, queryId: e.target.value }))}
                  style={{ marginBottom: '8px' }}
                />
                <Input
                  placeholder="Email Address"
                  value={brandedTypeDemo.email}
                  onChange={(e) => setBrandedTypeDemo(prev => ({ ...prev, email: e.target.value }))}
                  style={{ marginBottom: '8px' }}
                />
                <Input
                  placeholder="Confidence Score (0-1)"
                  value={brandedTypeDemo.confidence}
                  onChange={(e) => setBrandedTypeDemo(prev => ({ ...prev, confidence: e.target.value }))}
                  style={{ marginBottom: '8px' }}
                />
              </div>

              <Button
                type="primary"
                onClick={testBrandedTypes}
                icon={<BugOutlined />}
                block
              >
                Test Branded Types
              </Button>

              <Button
                onClick={testTypeGuards}
                icon={<CheckCircleOutlined />}
                block
              >
                Test Type Guards
              </Button>

              <Alert
                message="Branded Types Benefits"
                description="Branded types prevent mixing different string/number types that represent different concepts, catching bugs at compile time."
                type="info"

              />
            </Space>
          </Card>
        </Col>

        {/* Validated API Calls Demo */}
        <Col xs={24} lg={12}>
          <Card title="Validated API Calls Demo" style={{ height: '400px' }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Alert
                message="React Query + Zod Integration"
                description="API responses are automatically validated against Zod schemas."
                type="info"

              />

              <div>
                <Text strong>Query Status:</Text>
                <div>
                  <Tag color={isLoading ? 'blue' : error ? 'red' : 'green'}>
                    {isLoading ? 'Loading' : error ? 'Error' : 'Ready'}
                  </Tag>
                </div>
              </div>

              {queryData && (
                <div>
                  <Text strong>Validated Query Data:</Text>
                  <pre style={{
                    background: '#f5f5f5',
                    padding: '8px',
                    borderRadius: '4px',
                    fontSize: '12px',
                    maxHeight: '150px',
                    overflow: 'auto'
                  }}>
                    {JSON.stringify(queryData, null, 2)}
                  </pre>
                </div>
              )}

              <Button
                type="primary"
                onClick={() => executeQueryMutation.mutate({
                  naturalLanguageQuery: 'Show me all users',
                  includeExplanation: true
                })}
                loading={executeQueryMutation.isPending}
                block
              >
                Execute Validated Query
              </Button>
            </Space>
          </Card>
        </Col>

        {/* Form Validation Demo */}
        <Col xs={24} lg={12}>
          <Card title="Form Validation Demo" style={{ height: '400px' }}>
            <Form
              form={form}
              layout="vertical"
              onFinish={handleFormSubmit}
              initialValues={{
                theme: 'light',
                notifications: true
              }}
            >
              <Form.Item
                name="name"
                label="Name"
                rules={[{ required: true, message: 'Name is required' }]}
              >
                <Input placeholder="Enter name" />
              </Form.Item>

              <Form.Item
                name="email"
                label="Email"
                rules={[
                  { required: true, message: 'Email is required' },
                  { type: 'email', message: 'Invalid email format' }
                ]}
              >
                <Input placeholder="Enter email" />
              </Form.Item>

              <Form.Item
                name="age"
                label="Age"
                rules={[
                  { required: true, message: 'Age is required' },
                  { type: 'number', min: 0, max: 150, message: 'Age must be between 0 and 150' }
                ]}
              >
                <Input type="number" placeholder="Enter age" />
              </Form.Item>

              <Form.Item name="isActive" label="Active" valuePropName="checked">
                <Switch />
              </Form.Item>

              <Form.Item name="theme" label="Theme">
                <Select>
                  <Select.Option value="light">Light</Select.Option>
                  <Select.Option value="dark">Dark</Select.Option>
                </Select>
              </Form.Item>

              <Form.Item>
                <Button type="primary" htmlType="submit" block>
                  Validate Form with Zod
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </Col>
      </Row>

      {/* Advanced Features */}
      <Row style={{ marginTop: '16px' }}>
        <Col span={24}>
          <Collapse>
            <Panel header="Advanced Type Safety Features" key="1">
              <Row gutter={[16, 16]}>
                <Col xs={24} md={8}>
                  <Card size="small" title="Runtime Type Checking">
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Automatic validation of API responses
                    </Paragraph>
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Type guards for runtime safety
                    </Paragraph>
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Fallback values for invalid data
                    </Paragraph>
                  </Card>
                </Col>

                <Col xs={24} md={8}>
                  <Card size="small" title="Enhanced TypeScript Config">
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Strict null checks enabled
                    </Paragraph>
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> No implicit any types
                    </Paragraph>
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Exact optional property types
                    </Paragraph>
                  </Card>
                </Col>

                <Col xs={24} md={8}>
                  <Card size="small" title="Branded Types">
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Prevent ID type mixing
                    </Paragraph>
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Type-safe numeric ranges
                    </Paragraph>
                    <Paragraph>
                      <CheckCircleOutlined style={{ color: '#52c41a' }} /> Domain-specific types
                    </Paragraph>
                  </Card>
                </Col>
              </Row>
            </Panel>
          </Collapse>
        </Col>
      </Row>
    </div>
  );
};
