import express from 'express';
import { connectDB } from './db';

const app = express();

app.use(express.json());

app.post('/', async (req, res) => {
  const client = await connectDB();
  const { data } = req.body;

  await client.query(`INSERT INTO entries(data) VALUES ($1)`, [data]);

  const result = await client.query(`SELECT COUNT(*) FROM entries;`);
  const count = result.rows[0].count;

  res.status(201).send({ message: 'Entry added successfully.', count: count });
  await client.end();
});

export default app;
