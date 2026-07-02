const row = { id: '1', name: 'Test', post_count: 5 }; const postCountRaw = row.postCount ?? row.PostCount ?? row.post_count; console.log(typeof postCountRaw === 'number' ? postCountRaw : 0);
