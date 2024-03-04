import express from 'express';
import { connectDB } from './db';
import { v4 as uuidv4 } from 'uuid';

const app = express();

app.get('/', async (req, res) => {
  const client = await connectDB();
  const data = uuidv4();

  await client.query(`INSERT INTO entries(data) VALUES ($1)`, [data]);

  const result = await client.query(`SELECT COUNT(*) FROM entries;`);
  const count = result.rows[0].count;

  res.status(201).send({ message: 'Entry added successfully with random data.', data: data, count: count });
  await client.end();
});

app.get('/status', (req, res) => {
  res.status(200).send({ status: 'UP' });
});

export default app;
