import replace from 'replace-in-file';

const options = {
  files: 'dist/**/*.js',
  from: /__API_URL__/g,
  to: process.env.API_URL || 'http://localhost:8000',
};

replace(options)
  .then(results => {
    console.log('Replaced API URLs:', results);
  })
  .catch(error => {
    console.error('Error replacing API URLs:', error);
  });
